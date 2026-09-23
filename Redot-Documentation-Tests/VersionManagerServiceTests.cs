using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class VersionManagerServiceTests : IDisposable
{
    private readonly List<string> _contentRootPaths = [];

    [Fact]
    public void LoadContent_LoadsMetadataRolesAndCaseInsensitiveLookups()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion("26.1", "Redot 26.1", "26.1", isLatestStable: true)
        ];
        var (manager, contentRootPath) = CreateManager(versions);

        manager.LoadContent();

        Assert.Collection(
            manager.Versions,
            version =>
            {
                Assert.Equal("latest", version.Slug);
                Assert.Equal("Latest development", version.FriendlyName);
                Assert.Equal("master", version.BranchName);
            },
            version => Assert.Equal("26.1", version.Slug));
        Assert.Equal("26.1", manager.LatestStableVersion.Slug);
        Assert.Equal("latest", manager.NextPrereleaseVersion.Slug);
        Assert.True(manager.TryGetVersionProvider("LATEST", out VersionProvider? provider));
        Assert.NotNull(provider);
        Assert.Equal(
            Path.Combine(contentRootPath, "docs", "latest"),
            provider.VersionRoot);
        Assert.Equal(
            "/en/latest/example.md",
            provider.GetReferentialPath(Path.Combine(provider.VersionRoot, "example.md")));
    }

    [Fact]
    public void LoadContent_RepositoryDocumentationHasUniqueSlugs()
    {
        DirectoryInfo? repository = new(AppContext.BaseDirectory);
        while (repository != null && !File.Exists(Path.Combine(repository.FullName, "Redot-Documentation.sln")))
            repository = repository.Parent;

        Assert.NotNull(repository);
        var manager = new VersionManagerService(
            new TestWebHostEnvironment(Path.Combine(repository.FullName, "Redot-Documentation")));

        manager.LoadContent();

        VersionProvider provider = manager.GetVersionProvider("latest");
        Assert.Equal("/en/Community/tutorials", provider.GetPathFromSlug("doc_tutorials"));
        Assert.Equal("/en/latest/Tutorials/index", provider.GetPathFromSlug("doc_tutorials_overview"));
        Assert.Equal("/en/latest/Tutorials/math/interpolation", provider.GetPathFromSlug("doc_interpolation"));
        Assert.Equal("/en/latest/Tutorials/physics/interpolation/index", provider.GetPathFromSlug("doc_physics_interpolation"));
        Assert.Equal("/en/latest/Tutorials/editor/index", provider.GetPathFromSlug("doc_editor"));
        Assert.Equal("/en/latest/Tutorials/plugins/editor/index", provider.GetPathFromSlug("doc_editor_plugins"));
        Assert.Equal("/en/latest/Tutorials/plugins/editor/making_plugins", provider.GetPathFromSlug("doc_making_plugins"));
    }

    [Fact]
    public void LoadContent_DuplicateArticleAndSectionSlugsIdentifyBothPaths()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion("26.1", "Redot 26.1", "26.1", isLatestStable: true)
        ];
        var (manager, contentRootPath) = CreateManager(versions);
        string versionRoot = Path.Combine(contentRootPath, "docs", "latest");
        Directory.CreateDirectory(Path.Combine(versionRoot, "example"));
        File.WriteAllText(Path.Combine(versionRoot, "example.md"), "# Example");
        File.WriteAllText(Path.Combine(versionRoot, "example", "index.md"), "# Example section");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("Duplicate documentation slug 'doc_example' in version 'latest'", exception.Message);
        Assert.Contains("/en/latest/example", exception.Message);
        Assert.Contains("/en/latest/example/index", exception.Message);
    }

    [Fact]
    public void LoadContent_RejectsLegacyStringArray()
    {
        var manager = CreateManager("""
                                    [
                                      "latest",
                                      "26.1"
                                    ]
                                    """, "latest", "26.1");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("is invalid", exception.Message);
    }

    [Fact]
    public void LoadContent_RequiresExactlyOneVersionForEachRole()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion("26.1", "Redot 26.1", "26.1")
        ];
        var (manager, _) = CreateManager(versions);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("Exactly one documentation version must be the latest stable version", exception.Message);
    }

    [Fact]
    public void LoadContent_RejectsDuplicateSlugsIgnoringCase()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion("LATEST", "Redot 26.1", "26.1", isLatestStable: true)
        ];
        var (manager, _) = CreateManager(versions);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("slug 'LATEST' is duplicated", exception.Message);
    }

    [Theory]
    [InlineData("release/26.1")]
    [InlineData("../26.1")]
    [InlineData("version name")]
    public void LoadContent_RejectsUnsafeSlugs(string unsafeSlug)
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion(unsafeSlug, "Redot 26.1", "26.1", isLatestStable: true)
        ];
        var (manager, _) = CreateManager(versions);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("single URL- and path-safe segment", exception.Message);
    }

    [Fact]
    public void LoadContent_RejectsVersionAssignedToBothRoles()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion(
                "26.1",
                "Redot 26.1",
                "26.1",
                isLatestStable: true,
                isNextPrerelease: true)
        ];
        var (manager, _) = CreateManager(versions);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("cannot be both", exception.Message);
    }

    [Fact]
    public void LoadContent_RejectsMissingVersionDirectory()
    {
        DocumentationVersion[] versions =
        [
            CreateVersion("latest", "Latest development", "master", isNextPrerelease: true),
            CreateVersion("26.1", "Redot 26.1", "26.1", isLatestStable: true)
        ];
        var (manager, contentRootPath) = CreateManager(versions);
        Directory.Delete(Path.Combine(contentRootPath, "docs", "26.1"));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(manager.LoadContent);

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public void LoadContent_RejectsMissingConfigurationFile()
    {
        string contentRootPath = CreateContentRoot();
        Directory.CreateDirectory(Path.Combine(contentRootPath, "docs"));
        var manager = new VersionManagerService(new TestWebHostEnvironment(contentRootPath));

        Assert.Throws<FileNotFoundException>(manager.LoadContent);
    }

    public void Dispose()
    {
        foreach (string contentRootPath in _contentRootPaths)
        {
            if (Directory.Exists(contentRootPath))
                Directory.Delete(contentRootPath, recursive: true);
        }
    }

    private (VersionManagerService Manager, string ContentRootPath) CreateManager(
        IReadOnlyCollection<DocumentationVersion> versions)
    {
        string json = JsonSerializer.Serialize(versions);
        VersionManagerService manager = CreateManager(json, versions.Select(version => version.Slug).ToArray());
        return (manager, _contentRootPaths[^1]);
    }

    private VersionManagerService CreateManager(string json, params string[] versionDirectories)
    {
        string contentRootPath = CreateContentRoot();
        string docsRootPath = Path.Combine(contentRootPath, "docs");
        Directory.CreateDirectory(docsRootPath);
        foreach (string versionDirectory in versionDirectories)
        {
            string safeDirectoryName = versionDirectory.Replace('/', '_');
            Directory.CreateDirectory(Path.Combine(docsRootPath, safeDirectoryName));
        }

        File.WriteAllText(Path.Combine(docsRootPath, "Versions.json"), json);
        return new VersionManagerService(new TestWebHostEnvironment(contentRootPath));
    }

    private string CreateContentRoot()
    {
        string contentRootPath = Path.Combine(
            Path.GetTempPath(),
            $"redot-version-manager-tests-{Guid.NewGuid():N}");
        _contentRootPaths.Add(contentRootPath);
        return contentRootPath;
    }

    private static DocumentationVersion CreateVersion(
        string slug,
        string friendlyName,
        string branchName,
        bool isLatestStable = false,
        bool isNextPrerelease = false)
        => new()
        {
            Slug = slug,
            FriendlyName = friendlyName,
            BranchName = branchName,
            IsLatestStable = isLatestStable,
            IsNextPrerelease = isNextPrerelease
        };

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
