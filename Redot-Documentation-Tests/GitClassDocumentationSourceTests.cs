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
                var node = Assert.Single(new ClassDocumentationParser().ParseDirectory(recovered.ClassDocumentationPath)).Value;
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
