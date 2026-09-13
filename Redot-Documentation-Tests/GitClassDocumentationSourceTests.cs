using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class GitClassDocumentationSourceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"redot-class-git-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task PrepareAsync_ClonesOnlyConfiguredContentAndReusesTheCurrentCommit()
    {
        string remote = CreateRemoteRepository(out _);
        GitClassDocumentationSource source = CreateSource(remote);
        DocumentationVersion version = CreateVersion();

        using (ClassDocumentationCheckout candidate = await source.PrepareAsync(version, CancellationToken.None))
        {
            Assert.True(candidate.IsPending);
            Assert.True(File.Exists(Path.Combine(candidate.ClassDocumentationPath, "Node.xml")));
            Assert.False(File.Exists(Path.Combine(candidate.RepositoryPath, "large", "ignored.txt")));
            candidate.Promote();
        }

        using ClassDocumentationCheckout current = await source.PrepareAsync(version, CancellationToken.None);
        Assert.False(current.IsPending);
        Assert.True(File.Exists(Path.Combine(current.ClassDocumentationPath, "Node.xml")));
        Assert.True(source.TryGetCurrent(version, out ClassDocumentationCheckout? cached));
        cached!.Dispose();
    }

    [Fact]
    public async Task UnpromotedCandidate_LeavesTheLastValidCheckoutActive()
    {
        string remote = CreateRemoteRepository(out string sourcePath);
        GitClassDocumentationSource source = CreateSource(remote);
        DocumentationVersion version = CreateVersion();
        var parser = new ClassDocumentationParser();

        using (ClassDocumentationCheckout initial = await source.PrepareAsync(version, CancellationToken.None))
        {
            parser.ParseDirectory(initial.ClassDocumentationPath);
            initial.Promote();
        }

        File.WriteAllText(Path.Combine(sourcePath, "doc", "classes", "Node.xml"), "<not-a-class />");
        RunGit(sourcePath, "add", ".");
        RunGit(sourcePath, "commit", "-m", "Invalid class docs");
        RunGit(sourcePath, "push", remote, "master");

        using (ClassDocumentationCheckout invalid = await source.PrepareAsync(version, CancellationToken.None))
        {
            Assert.True(invalid.IsPending);
            Assert.Throws<InvalidDataException>(() => parser.ParseDirectory(invalid.ClassDocumentationPath));
        }

        Assert.True(source.TryGetCurrent(version, out ClassDocumentationCheckout? current));
        using (current)
        {
            ClassDocumentationEntry node = Assert.Single(parser.ParseDirectory(current!.ClassDocumentationPath)).Value;
            Assert.Equal("Node", node.Name);
        }
    }

    [Fact]
    public async Task PrepareAsync_RecreatesCheckoutWhenClassDocumentationDirectoryIsMissing()
    {
        string remote = CreateRemoteRepository(out _);
        GitClassDocumentationSource source = CreateSource(remote);
        DocumentationVersion version = CreateVersion();

        using (ClassDocumentationCheckout initial = await source.PrepareAsync(version, CancellationToken.None))
        {
            initial.Promote();
        }

        string classDocumentationPath = Path.Combine(
            _root,
            "cache",
            version.Slug,
            "repository",
            "doc",
            "classes");
        Directory.Delete(classDocumentationPath, recursive: true);

        using ClassDocumentationCheckout recreated = await source.PrepareAsync(version, CancellationToken.None);

        Assert.True(recreated.IsPending);
        Assert.True(File.Exists(Path.Combine(recreated.ClassDocumentationPath, "Node.xml")));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    public async Task InterruptedPromotion_RecoversMatchingCheckoutAndMetadata(bool hasPrevious, int phase)
    {
        string remote = CreateRemoteRepository(out string sourcePath);
        var source = CreateSource(remote);
        var version = CreateVersion();
        string? previousCommit = null;
        if (hasPrevious)
        {
            using var initial = await source.PrepareAsync(version, CancellationToken.None);
            initial.Promote();
            previousCommit = initial.CommitSha;
        }

        File.WriteAllText(Path.Combine(sourcePath, "doc", "classes", "Node.xml"),
            "<class name=\"Node\"><brief_description>Updated node.</brief_description></class>");
        AddModule(sourcePath, "alpha", "Alpha");
        RunGit(sourcePath, "add", ".");
        RunGit(sourcePath, "commit", "-m", "Update class docs");
        RunGit(sourcePath, "push", remote, "master");

        using var candidate = await source.PrepareAsync(version, CancellationToken.None);
        string versionRoot = Path.Combine(_root, "cache", version.Slug);
        string active = Path.Combine(versionRoot, "repository");
        string metadataPath = Path.Combine(versionRoot, "sync.json");

        // Simulate process termination at each filesystem boundary, without running
        // Promote's exception rollback or replacing the old metadata prematurely.
        if (phase >= 1 && hasPrevious)
            Directory.Move(active, Path.Combine(versionRoot, "repository.previous"));
        if (phase >= 2)
            Directory.Move(candidate.RepositoryPath, active);
        if (phase >= 3)
            File.Copy(Path.Combine(active, ".redot-class-doc-sync.json"), metadataPath, overwrite: true);

        string expectedCommit = phase >= 2 ? candidate.CommitSha : previousCommit!;
        var restarted = CreateSource(remote);
        for (int attempt = 0; attempt < 2; attempt++)
        {
            Assert.True(restarted.TryGetCurrent(version, out var recovered));
            using (recovered)
            {
                Assert.Equal(expectedCommit, recovered!.CommitSha);
                var classes = new ClassDocumentationParser().ParseDirectories(recovered.ClassDocumentationPaths);
                Assert.Equal(phase >= 2, classes.ContainsKey("Alpha"));
                var node = classes["Node"];
                Assert.Equal(phase >= 2 ? "Updated node." : "A node.", node.BriefDescription);
            }
            using var metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
            Assert.Equal(expectedCommit, metadata.RootElement.GetProperty("CommitSha").GetString());
        }

        using var prepared = await restarted.PrepareAsync(version, CancellationToken.None);
        Assert.Equal(phase < 2, prepared.IsPending);
        Assert.Equal(candidate.CommitSha, prepared.CommitSha);
    }

    [Fact]
    public async Task TryGetCurrent_SupportsLegacyMetadataButDoesNotFallBackFromCorruptCheckoutMetadata()
    {
        string remote = CreateRemoteRepository(out _);
        var source = CreateSource(remote);
        var version = CreateVersion();
        using var initial = await source.PrepareAsync(version, CancellationToken.None);
        initial.Promote();
        string metadata = Path.Combine(_root, "cache", version.Slug, "repository", ".redot-class-doc-sync.json");
        File.Delete(metadata);
        Assert.True(source.TryGetCurrent(version, out var legacy));
        legacy!.Dispose();

        File.WriteAllText(metadata, "invalid metadata");
        Assert.False(source.TryGetCurrent(version, out _));
    }

    [Fact]
    public async Task Modules_AreDiscoveredAcrossUpdatesAndMissingCachedDirectoriesAreRepaired()
    {
        string remote = CreateRemoteRepository(out string work);
        AddModule(work, "alpha", "Alpha");
        File.WriteAllText(Path.Combine(work, "modules", "alpha", "source.cpp"), "not documentation");
        File.WriteAllText(Path.Combine(work, "modules", "alpha", "unrelated.xml"), "<not-a-class />");
        CommitAndPush(work, remote);
        var source = CreateSource(remote);
        var version = CreateVersion();
        var parser = new ClassDocumentationParser();
        using (var candidate = await source.PrepareAsync(version, CancellationToken.None))
        {
            Assert.Equal(2, candidate.ClassDocumentationPaths.Count);
            Assert.Equal(2, parser.ParseDirectories(candidate.ClassDocumentationPaths).Count);
            Assert.False(File.Exists(Path.Combine(candidate.RepositoryPath, "modules", "alpha", "source.cpp")));
            Assert.False(File.Exists(Path.Combine(candidate.RepositoryPath, "modules", "alpha", "unrelated.xml")));
            candidate.Promote();
        }
        Assert.True(source.TryGetCurrent(version, out var cached));
        using (cached)
            Assert.Contains("Alpha", parser.ParseDirectories(cached!.ClassDocumentationPaths).Keys);

        Directory.Delete(Path.Combine(_root, "cache", "latest", "repository", "modules", "alpha", "doc_classes"), true);
        Assert.False(source.TryGetCurrent(version, out _));
        using (var repair = await source.PrepareAsync(version, CancellationToken.None))
        {
            Assert.True(repair.IsPending);
            Assert.Contains("Alpha", parser.ParseDirectories(repair.ClassDocumentationPaths).Keys);
            repair.Promote();
        }

        Directory.Delete(Path.Combine(work, "modules", "alpha"), true);
        AddModule(work, "new_module", "NewModule");
        CommitAndPush(work, remote);
        using var updated = await source.PrepareAsync(version, CancellationToken.None);
        var classes = parser.ParseDirectories(updated.ClassDocumentationPaths);
        Assert.Contains("NewModule", classes.Keys);
        Assert.DoesNotContain("Alpha", classes.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentSelection_WithOmittedModuleDirectory_IsRejectedAndRepaired(bool hasSecondModule)
    {
        string remote = CreateRemoteRepository(out string work);
        AddModule(work, "alpha", "Alpha");
        if (hasSecondModule)
            AddModule(work, "beta", "Beta");
        CommitAndPush(work, remote);
        var source = CreateSource(remote);
        var version = CreateVersion();
        using (var initial = await source.PrepareAsync(version, CancellationToken.None))
            initial.Promote();

        string repository = Path.Combine(_root, "cache", "latest", "repository");
        string marker = Path.Combine(repository, ".redot-class-doc-sync.json");
        var metadata = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(marker))!.AsObject();
        int revision = metadata["SelectionRevision"]!.GetValue<int>();
        var directories = metadata["DocumentationDirectories"]!.AsArray();
        directories.Remove(directories.Single(path => path!.GetValue<string>() == "modules/alpha/doc_classes"));
        File.WriteAllText(marker, metadata.ToJsonString());

        Assert.True(revision > 0);
        Assert.True(Directory.Exists(Path.Combine(repository, "modules", "alpha", "doc_classes")));
        Assert.False(source.TryGetCurrent(version, out _));

        using var repair = await source.PrepareAsync(version, CancellationToken.None);
        Assert.True(repair.IsPending);
        Assert.Equal(metadata["CommitSha"]!.GetValue<string>(), repair.CommitSha);
        Assert.Equal(hasSecondModule ? 3 : 2, repair.ClassDocumentationPaths.Count);
        Assert.Contains("Alpha", new ClassDocumentationParser().ParseDirectories(repair.ClassDocumentationPaths).Keys);
        repair.Promote();
        using var current = await source.PrepareAsync(version, CancellationToken.None);
        Assert.False(current.IsPending);
        var repairedMetadata = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(marker))!;
        Assert.Equal(revision, repairedMetadata["SelectionRevision"]!.GetValue<int>());
    }

    [Fact]
    public async Task LegacyCoreOnlyCache_IsReadableButRefreshedAtTheSameCommit()
    {
        string remote = CreateRemoteRepository(out string work);
        AddModule(work, "alpha", "Alpha");
        CommitAndPush(work, remote);
        var source = CreateSource(remote);
        var version = CreateVersion();
        using (var initial = await source.PrepareAsync(version, CancellationToken.None))
            initial.Promote();
        string root = Path.Combine(_root, "cache", "latest");
        string marker = Path.Combine(root, "repository", ".redot-class-doc-sync.json");
        var metadata = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(marker))!.AsObject();
        metadata.Remove("SelectionRevision");
        metadata.Remove("CoreDocumentationPath");
        metadata.Remove("DocumentationDirectories");
        File.WriteAllText(marker, metadata.ToJsonString());
        Directory.Delete(Path.Combine(root, "repository", "modules"), true);

        Assert.True(source.TryGetCurrent(version, out var cached));
        using (cached)
            Assert.Single(cached!.ClassDocumentationPaths);
        using var upgrade = await source.PrepareAsync(version, CancellationToken.None);
        Assert.True(upgrade.IsPending);
        Assert.Equal(metadata["CommitSha"]!.GetValue<string>(), upgrade.CommitSha);
        Assert.Contains("Alpha", new ClassDocumentationParser().ParseDirectories(upgrade.ClassDocumentationPaths).Keys);
        upgrade.Promote();
        using var current = await source.PrepareAsync(version, CancellationToken.None);
        Assert.False(current.IsPending);
    }

    [Theory]
    [InlineData("<not-a-class />")]
    [InlineData("<class name=\"Node\" />")]
    public async Task InvalidModule_LeavesPreviousSnapshotAvailable(string xml)
    {
        string remote = CreateRemoteRepository(out string work);
        var source = CreateSource(remote);
        var version = CreateVersion();
        using (var initial = await source.PrepareAsync(version, CancellationToken.None))
            initial.Promote();
        AddModule(work, "bad", "Bad");
        File.WriteAllText(Path.Combine(work, "modules", "bad", "doc_classes", "Bad.xml"), xml);
        CommitAndPush(work, remote);
        using (var candidate = await source.PrepareAsync(version, CancellationToken.None))
            Assert.Throws<InvalidDataException>(() => new ClassDocumentationParser().ParseDirectories(candidate.ClassDocumentationPaths));
        Assert.True(source.TryGetCurrent(version, out var cached));
        using (cached)
            Assert.Single(new ClassDocumentationParser().ParseDirectories(cached!.ClassDocumentationPaths));
    }

    [Fact]
    public async Task Versions_DiscoverTheirOwnModules()
    {
        string remote = CreateRemoteRepository(out string work);
        RunGit(work, "push", remote, "HEAD:refs/heads/26.1");
        AddModule(work, "new_feature", "NewFeature");
        CommitAndPush(work, remote);
        var source = CreateSource(remote);
        var stableVersion = new DocumentationVersion { Slug = "26.1", FriendlyName = "Stable", BranchName = "26.1" };
        using var stable = await source.PrepareAsync(stableVersion, CancellationToken.None);
        using var latest = await source.PrepareAsync(CreateVersion(), CancellationToken.None);
        var parser = new ClassDocumentationParser();
        Assert.Single(parser.ParseDirectories(stable.ClassDocumentationPaths));
        var latestClasses = parser.ParseDirectories(latest.ClassDocumentationPaths);
        Assert.Contains("NewFeature", latestClasses.Keys);
        var snapshot = new ClassDocumentationSnapshot(latest.Version, latest.CommitSha, latest.SynchronizedAt, latestClasses);
        var catalog = new ClassDocumentationCatalog();
        catalog.Publish(snapshot);
        Assert.Contains(catalog.GetClassesAlphabetically("latest"), entry => entry.Name == "NewFeature");
        Assert.True(catalog.TryGetClass("latest", "NewFeature", out var module));
        string html = new ClassDocumentationRenderer().RenderPage(module!, snapshot);
        Assert.Contains("/en/latest/Classes/Node", html);
    }

    private static void AddModule(string work, string module, string name)
    {
        string path = Path.Combine(work, "modules", module, "doc_classes");
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, name + ".xml"), $"<class name=\"{name}\" inherits=\"Node\" />");
    }

    private static void CommitAndPush(string work, string remote)
    {
        RunGit(work, "add", ".");
        RunGit(work, "commit", "-m", "Update modules");
        RunGit(work, "push", remote, "master");
    }

    private GitClassDocumentationSource CreateSource(string repositoryUrl)
    {
        var options = Options.Create(new ClassDocumentationOptions
        {
            RepositoryUrl = repositoryUrl,
            RepositoryPath = "doc/classes",
            CacheRoot = "cache",
            GitTimeout = TimeSpan.FromSeconds(30)
        });
        return new GitClassDocumentationSource(
            new GitCommandRunner(),
            options,
            new TestWebHostEnvironment(_root),
            NullLogger<GitClassDocumentationSource>.Instance);
    }

    private string CreateRemoteRepository(out string sourcePath)
    {
        sourcePath = Path.Combine(_root, $"source-{Guid.NewGuid():N}");
        string remotePath = Path.Combine(_root, "remote.git");
        Directory.CreateDirectory(Path.Combine(sourcePath, "doc", "classes"));
        Directory.CreateDirectory(Path.Combine(sourcePath, "large"));
        File.WriteAllText(
            Path.Combine(sourcePath, "doc", "classes", "Node.xml"),
            "<class name=\"Node\"><brief_description>A node.</brief_description></class>");
        File.WriteAllText(Path.Combine(sourcePath, "large", "ignored.txt"), "not sparse content");

        RunGit(sourcePath, "init", "--initial-branch=master");
        RunGit(sourcePath, "config", "user.name", "Class Docs Test");
        RunGit(sourcePath, "config", "user.email", "class-docs@example.invalid");
        RunGit(sourcePath, "add", ".");
        RunGit(sourcePath, "commit", "-m", "Initial class docs");
        RunGit(_root, "clone", "--bare", sourcePath, remotePath);
        return remotePath;
    }

    private static void RunGit(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);
        using Process process = Process.Start(startInfo)!;
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(process.StandardError.ReadToEnd());
    }

    private static DocumentationVersion CreateVersion()
        => new()
        {
            Slug = "latest",
            FriendlyName = "Latest development",
            BranchName = "master",
            IsNextPrerelease = true
        };

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
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
