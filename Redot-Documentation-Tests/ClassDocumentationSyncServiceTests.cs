using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class ClassDocumentationSyncServiceTests : IDisposable
{
    private readonly string _contentRootPath = Path.Combine(
        Path.GetTempPath(),
        $"redot-class-sync-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task StartAsync_DoesNotWaitForInitialSynchronizationAndCapturesItsFailure()
    {
        VersionManagerService versionManager = CreateVersionManager();
        var source = new BlockingFailingSource();
        var catalog = new ClassDocumentationCatalog();
        using var service = new ClassDocumentationSyncService(
            versionManager,
            source,
            new ClassDocumentationParser(),
            catalog,
            Options.Create(new ClassDocumentationOptions { RefreshInterval = TimeSpan.FromDays(1) }),
            NullLogger<ClassDocumentationSyncService>.Instance);

        Task startTask = service.StartAsync(CancellationToken.None);
        await source.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await startTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.False(catalog.TryGetSnapshot("latest", out _));
        source.Release.SetResult();
        await source.Failed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_RefreshesAfterCachedCheckoutAccessIsDenied()
    {
        VersionManagerService manager = CreateVersionManager();
        string classesPath = Path.Combine(_contentRootPath, "fresh-classes");
        Directory.CreateDirectory(classesPath);
        File.WriteAllText(Path.Combine(classesPath, "Node.xml"), "<class name=\"Node\" />");
        var source = new TestSource(classesPath) { DenyCacheAccess = true };
        var catalog = new ClassDocumentationCatalog();
        using var service = CreateService(manager, source, catalog);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        foreach (var version in manager.Versions)
            Assert.True(catalog.TryGetClass(version.Slug, "Node", out _));
        Assert.Equal(manager.Versions.Count, source.PrepareCount);
        Assert.Equal(manager.Versions.Count, source.PromotionCount);
    }

    [Theory]
    [InlineData("<class xmlns=\"urn:redot\" name=\"Node\" />")]
    [InlineData("<class name=\"Node\"><constants><constant name=\"READY\" /></constants></class>")]
    public async Task StartAsync_DoesNotPromoteOrPublishMalformedClassData(string xml)
    {
        VersionManagerService manager = CreateVersionManager();
        string classesPath = Path.Combine(_contentRootPath, "invalid-classes");
        Directory.CreateDirectory(classesPath);
        File.WriteAllText(Path.Combine(classesPath, "Node.xml"), xml);
        var source = new TestSource(classesPath);
        var catalog = new ClassDocumentationCatalog();
        var existing = new ClassDocumentationSnapshot(manager.LatestStableVersion, "previous", DateTimeOffset.UtcNow,
            new Dictionary<string, ClassDocumentationEntry> { ["Node"] = new() { Name = "Node" } });
        catalog.Publish(existing);
        using var service = CreateService(manager, source, catalog);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(manager.Versions.Count, source.PrepareCount);
        Assert.Equal(0, source.PromotionCount);
        Assert.True(catalog.TryGetSnapshot(manager.LatestStableVersion.Slug, out var actual));
        Assert.Same(existing, actual);
        Assert.False(catalog.TryGetSnapshot(manager.NextPrereleaseVersion.Slug, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartAsync_ValidatesCoreAndModulesBeforePublishingTogether(bool invalidModule)
    {
        var manager = CreateVersionManager();
        string core = Path.Combine(_contentRootPath, "core");
        string module = Path.Combine(_contentRootPath, "module");
        Directory.CreateDirectory(core);
        Directory.CreateDirectory(module);
        File.WriteAllText(Path.Combine(core, "Node.xml"), "<class name=\"Node\" />");
        File.WriteAllText(Path.Combine(module, "ModuleClass.xml"), invalidModule ? "<invalid />" : "<class name=\"ModuleClass\" />");
        var source = new TestSource(core) { DocumentationPaths = [core, module] };
        var catalog = new ClassDocumentationCatalog();
        using var service = CreateService(manager, source, catalog);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(invalidModule ? 0 : manager.Versions.Count, source.PromotionCount);
        foreach (var version in manager.Versions)
        {
            Assert.Equal(!invalidModule, catalog.TryGetSnapshot(version.Slug, out var snapshot));
            if (!invalidModule)
                Assert.Equal(["ModuleClass", "Node"], snapshot!.Classes.Keys.Order(StringComparer.Ordinal));
        }
    }

    private static ClassDocumentationSyncService CreateService(
        VersionManagerService manager, IClassDocumentationSource source, ClassDocumentationCatalog catalog)
        => new(manager, source, new ClassDocumentationParser(), catalog,
            Options.Create(new ClassDocumentationOptions { RefreshInterval = TimeSpan.FromDays(1) }),
            NullLogger<ClassDocumentationSyncService>.Instance);

    private sealed class TestSource(string classesPath) : IClassDocumentationSource
    {
        public bool DenyCacheAccess { get; init; }
        public IReadOnlyList<string>? DocumentationPaths { get; init; }
        public int PrepareCount { get; private set; }
        public int PromotionCount { get; private set; }

        public bool TryGetCurrent(DocumentationVersion version, out ClassDocumentationCheckout? checkout)
        {
            checkout = null;
            if (DenyCacheAccess)
                throw new UnauthorizedAccessException("Cached checkout is inaccessible.");
            return false;
        }

        public Task<ClassDocumentationCheckout> PrepareAsync(DocumentationVersion version, CancellationToken cancellationToken)
        {
            PrepareCount++;
            return Task.FromResult(ClassDocumentationCheckout.Pending(version, "new-commit", classesPath, classesPath,
                Path.Combine(classesPath, "unused-staging"), () => PromotionCount++, DocumentationPaths));
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, recursive: true);
    }

    private VersionManagerService CreateVersionManager()
    {
        DocumentationVersion[] versions =
        [
            new()
            {
                Slug = "latest",
                FriendlyName = "Latest development",
                BranchName = "master",
                IsNextPrerelease = true
            },
            new()
            {
                Slug = "26.1",
                FriendlyName = "Redot 26.1",
                BranchName = "26.1",
                IsLatestStable = true
            }
        ];
        string docsRootPath = Path.Combine(_contentRootPath, "docs");
        Directory.CreateDirectory(Path.Combine(docsRootPath, "latest"));
        Directory.CreateDirectory(Path.Combine(docsRootPath, "26.1"));
        File.WriteAllText(Path.Combine(docsRootPath, "Versions.json"), JsonSerializer.Serialize(versions));

        var manager = new VersionManagerService(new TestWebHostEnvironment(_contentRootPath));
        manager.LoadContent();
        return manager;
    }

    private sealed class BlockingFailingSource : IClassDocumentationSource
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Failed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<ClassDocumentationCheckout> PrepareAsync(
            DocumentationVersion version,
            CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            Failed.TrySetResult();
            throw new InvalidOperationException("Synchronization failed.");
        }

        public bool TryGetCurrent(
            DocumentationVersion version,
            out ClassDocumentationCheckout? checkout)
        {
            checkout = null;
            return false;
        }
    }

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Redot_Documentation_Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
