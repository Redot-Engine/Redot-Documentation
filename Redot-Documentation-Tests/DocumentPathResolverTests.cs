using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class DocumentPathResolverTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"redot-path-tests-{Guid.NewGuid():N}");
    private readonly VersionManagerService versions;
    private readonly IWebHostEnvironment environment;

    public DocumentPathResolverTests()
    {
        environment = new TestEnvironment(root);
        Write("26.1/Tutorials/Physics/ray-casting.md", "# Stable physics");
        Write("latest/Tutorials/Physics/ray-casting.md", "# Development physics");
        Write("About/Introduction.md", "# Shared introduction");
        Write("26.1/About/Introduction.md", "# Version introduction");
        Write("About/index.json", "{\"SlugPrefix\":\"abt_\"}");
        Write("26.1/Tutorials/Space ü.md", "# Unicode");
        Write("26.1/OnlyStable.md", "# Only stable");
        File.WriteAllText(Path.Combine(root, "docs", "Versions.json"), JsonSerializer.Serialize(new[]
        {
            new DocumentationVersion { Slug = "26.1", FriendlyName = "Stable", BranchName = "26.1", IsLatestStable = true },
            new DocumentationVersion { Slug = "latest", FriendlyName = "Development", BranchName = "master", IsNextPrerelease = true }
        }));
        versions = new VersionManagerService(environment);
        versions.LoadContent();
    }

    [Theory]
    [InlineData("26.1/tutorials/PHYSICS/RAY-CASTING")]
    [InlineData("26.1/tutorials/PHYSICS/RAY-CASTING.md")]
    [InlineData("26.1/tutorials/PHYSICS/RAY-CASTING.MD")]
    [InlineData("26.1/tutorials/PHYSICS/RAY-CASTING.html")]
    [InlineData("26.1/tutorials/PHYSICS/RAY-CASTING.HTML")]
    [InlineData("/26.1/tutorials/PHYSICS/RAY-CASTING.HTML/")]
    [InlineData("tutorials/physics/ray-casting.html")]
    public async Task AliasesResolveAndRenderTheSamePhysicalDocument(string route)
    {
        var paths = new DocumentPathResolver(environment, versions);
        var document = Assert.IsType<ResolvedDocument>(paths.ResolveRoute(route, versions));
        Assert.Equal(Path.Combine(root, "docs", "26.1", "Tutorials", "Physics", "ray-casting.md"), document.FullPath);
        Assert.Equal("/en/26.1/Tutorials/Physics/ray-casting", document.PublicUrl);
        var (provider, path) = DocumentPathResolver.ParseRoute(route, versions);
        var html = await new DocRendererService(paths).RenderToHtmlAsync(path, provider);
        Assert.Contains("Stable physics", html);
    }

    [Theory]
    [InlineData("about/INTRODUCTION.HTML")]
    [InlineData("26.1/about/INTRODUCTION.md")]
    [InlineData("LATEST/about/introduction")]
    public void SharedDocumentsKeepPrecedence(string route)
    {
        var document = new DocumentPathResolver(environment, versions).ResolveRoute(route, versions);
        Assert.Equal("About/Introduction.md", document?.RelativePath);
    }

    [Fact]
    public void VersionIsCaseInsensitiveButMissingContentDoesNotFallBackToAnotherVersion()
    {
        var paths = new DocumentPathResolver(environment, versions);
        Assert.Equal("latest/Tutorials/Physics/ray-casting.md",
            paths.ResolveRoute("LATEST/tutorials/physics/ray-casting.HTML", versions)?.RelativePath);
        Assert.Null(paths.ResolveRoute("latest/OnlyStable.html", versions));
        Assert.Null(paths.ResolveRoute("99.0/Tutorials/Physics/ray-casting", versions));
        Assert.Null(paths.ResolveRoute("26.1/missing.html", versions));
        Assert.Null(paths.ResolveRoute("26.1/Tutorials/Physics/ray-casting.htm", versions));
        Assert.Null(paths.ResolveRoute("26.1/Tutorials/Physics/ray-casting.html.md", versions));
        Assert.Null(paths.ResolveRoute("", versions));
    }

    [Theory]
    [InlineData("latest/26.1/OnlyStable.html")]
    [InlineData("LATEST/26.1/Tutorials/Physics/ray-casting.MD")]
    [InlineData("26.1/LATEST/Tutorials/Physics/ray-casting.HTML")]
    public void NestedVersionPrefixesCannotSelectAnotherVersionsDocument(string route)
    {
        var paths = new DocumentPathResolver(environment, versions);
        Assert.Null(paths.ResolveRoute(route, versions));
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("26.1/../../docs-other/secret.md")]
    [InlineData("26.1/Tutorials/./Physics/ray-casting")]
    [InlineData("26.1\\Tutorials\\Physics\\ray-casting")]
    [InlineData("26.1/file.md?x=1")]
    [InlineData("26.1/file.html#Heading")]
    [InlineData("26.1/file\0.md")]
    public void RejectsInvalidPaths(string route)
        => Assert.Throws<ArgumentException>(() => new DocumentPathResolver(environment, versions).ResolveRoute(route, versions));

    [Fact]
    public void EncodedCharactersAreDecodedOnlyAtTheUrlBoundary()
    {
        var paths = new DocumentPathResolver(environment, versions);
        var route = Uri.UnescapeDataString("26.1/tutorials/space%20%C3%BC.HTML");
        Assert.Equal("/en/26.1/Tutorials/Space%20%C3%BC", paths.ResolveRoute(route, versions)?.PublicUrl);
        Assert.Null(paths.ResolveRoute("26.1/%2e%2e/About/Introduction", versions));
    }

    [Fact]
    public void CaseCollisionsIdentifyBothFiles()
    {
        // Probe the actual test volume before creating a second, differently cased file.
        if (File.Exists(Path.Combine(root, "docs", "26.1", "tutorials", "physics", "RAY-CASTING.md")))
            return;
        Write("26.1/tutorials/physics/RAY-CASTING.md", "# Collision");
        var error = Assert.Throws<InvalidOperationException>(() => new DocumentPathResolver(environment, versions));
        Assert.Contains("26.1/Tutorials/Physics/ray-casting.md", error.Message);
        Assert.Contains("26.1/tutorials/physics/RAY-CASTING.md", error.Message);
    }

    [Fact]
    public void DoesNotIndexNonMarkdownOrSymbolicLinks()
    {
        Write("26.1/private.txt", "Private");
        if (!OperatingSystem.IsWindows())
            File.CreateSymbolicLink(Path.Combine(root, "docs", "26.1", "alias.md"),
                Path.Combine(root, "docs", "26.1", "private.txt"));
        var paths = new DocumentPathResolver(environment, versions);
        Assert.Null(paths.ResolveRoute("26.1/private.txt", versions));
        Assert.Null(paths.ResolveRoute("26.1/alias", versions));
    }

    [Theory]
    [InlineData("../../../docs-private/secret.md")]
    [InlineData("26.1/../../docs-private/secret.html")]
    [InlineData("26.1/%2e%2e/%2e%2e/docs-private/secret.md")]
    [InlineData("26.1/%2E%2E%2F%2E%2E%2Fdocs-private%2Fsecret.HTML")]
    [InlineData("26.1/%252e%252e/%252e%252e/docs-private/secret.md")]
    [InlineData("26.1/..%5c..%5cdocs-private%5csecret.md")]
    [InlineData("26.1/..%255c..%255cdocs-private%255csecret.md")]
    [InlineData("26.1/..;/..;/docs-private/secret.md")]
    [InlineData("26.1/%c0%ae%c0%ae/%c0%ae%c0%ae/docs-private/secret.md")]
    [InlineData("26.1/../../docs-private/secret.md%00.html")]
    public async Task TraversalCannotRenderExistingFilesOutsideDocs(string route)
    {
        var outside = Path.Combine(root, "docs-private");
        Directory.CreateDirectory(outside);
        await File.WriteAllTextAsync(Path.Combine(outside, "secret.md"), "OUTSIDE_DOCS_SENTINEL");
        var paths = new DocumentPathResolver(environment, versions);
        var renderer = new DocRendererService(paths);
        // Check raw, once-decoded and twice-decoded forms independently: none may
        // reach the existing sibling file, regardless of upstream URL decoding.
        for (var pass = 0; pass < 3; pass++)
        {
            var (provider, path) = DocumentPathResolver.ParseRoute(route, versions);
            var error = await Record.ExceptionAsync(() => renderer.RenderToHtmlAsync(path, provider));
            Assert.True(error is ArgumentException or FileNotFoundException,
                $"Traversal was not rejected: {route}; exception: {error}");
            route = Uri.UnescapeDataString(route);
        }
        var absoluteError = await Record.ExceptionAsync(() => renderer.RenderToHtmlAsync(
            Path.Combine(outside, "secret.md"), versions.GetVersionProvider("26.1")));
        Assert.True(absoluteError is ArgumentException or FileNotFoundException);
    }

    [Fact]
    public void DoesNotFollowFileOrDirectoryLinksOutsideDocs()
    {
        if (OperatingSystem.IsWindows()) return;
        var outside = Path.Combine(root, "docs-private");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "secret.md"), "OUTSIDE_DOCS_SENTINEL");
        File.CreateSymbolicLink(Path.Combine(root, "docs", "26.1", "external.md"),
            Path.Combine(outside, "secret.md"));
        Directory.CreateSymbolicLink(Path.Combine(root, "docs", "26.1", "external-folder"), outside);
        var paths = new DocumentPathResolver(environment, versions);
        Assert.Null(paths.ResolveRoute("26.1/external.HTML", versions));
        Assert.Null(paths.ResolveRoute("26.1/external-folder/secret.HTML", versions));
    }

    private void Write(string path, string text)
    {
        var file = Path.Combine(root, "docs", path);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, text);
    }

    public void Dispose() => Directory.Delete(root, true);

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = root;
        public string WebRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
