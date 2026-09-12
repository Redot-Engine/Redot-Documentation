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
