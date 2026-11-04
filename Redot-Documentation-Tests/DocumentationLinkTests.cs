using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Redot_Documentation.Services;
using System.Text.RegularExpressions;

namespace Redot_Documentation_Tests;

public sealed class DocumentationLinkTests
{
    [Fact]
    public async Task AllDocumentationLinksResolveToExistingPagesAndSections()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Redot-Documentation", "docs", "Versions.json")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        var root = Path.Combine(directory.FullName, "Redot-Documentation");
        var environment = new AuditEnvironment { ContentRootPath = root };
        var versions = new VersionManagerService(environment);
        versions.LoadContent();
        var paths = new DocumentPathResolver(environment, versions);
        var diagrams = new MermaidDiagramCatalog(environment, versions, NullLogger<MermaidDiagramCatalog>.Instance);
        Assert.True(diagrams.Errors.Count == 0, string.Join(Environment.NewLine, diagrams.Errors));
        var renderer = new DocRendererService(paths, diagrams);
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var pages = new Dictionary<(string Version, string Route), HtmlDocument>();
        foreach (var version in versions.Versions)
        {
            var provider = versions.GetVersionProvider(version.Slug);
            foreach (var file in DocumentPathResolver.EnumerateMarkdownFiles(Path.Combine(root, "docs")))
            {
                var relative = Path.GetRelativePath(Path.Combine(root, "docs"), file).Replace('\\', '/');
                if (!relative.StartsWith(version.Slug + "/") &&
                    !new[] { "About/", "Community/", "Contributing/" }.Any(relative.StartsWith))
                    continue;
                var resolved = paths.ResolveRoute(relative, versions);
                Assert.NotNull(resolved);
                Assert.Equal(Path.GetFullPath(file), resolved.FullPath);
                // Verify legacy aliases against the merged documentation corpus as well.
                foreach (var suffix in new[] { "", ".MD", ".HTML" })
                    Assert.Equal(resolved, paths.ResolveRoute(relative[..^3].ToUpperInvariant() + suffix, versions));
                var document = new HtmlDocument();
                document.LoadHtml(DocumentHeadings.Apply(await renderer.RenderToHtmlAsync(resolved, provider), []));
                Assert.True(document.DocumentNode.SelectSingleNode("//*[@class='doc-mermaid-error']") is null,
                    version.Slug + ": " + relative + " contains an invalid Mermaid embed.");
                foreach (var image in document.DocumentNode.SelectNodes("//figure[@class='doc-mermaid']//img") ?? Enumerable.Empty<HtmlNode>())
                {
                    var source = image.GetAttributeValue("src", "");
                    var match = Regex.Match(source, @"^/_diagrams/([a-f0-9]{64})\.svg$");
                    Assert.True(match.Success, version.Slug + ": " + relative + " has an unexpected Mermaid image source: " + source);
                    var context = new DefaultHttpContext { RequestServices = services };
                    context.Request.Method = "GET";
                    using var body = new MemoryStream();
                    context.Response.Body = body;
                    await diagrams.Image(match.Groups[1].Value, context).ExecuteAsync(context);
                    Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
                    Assert.Equal("image/svg+xml", context.Response.ContentType);
                    Assert.True(body.Length > 0, source + " returned an empty SVG.");
                }
                pages.Add((version.Slug, resolved.PublicUrl), document);
            }
        }

        var errors = new List<string>();
        foreach (var (key, page) in pages)
            foreach (var link in page.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
            {
                var href = HtmlEntity.DeEntitize(link.GetAttributeValue("href", ""));
                if (string.IsNullOrEmpty(href) || href.StartsWith("//") ||
                    Uri.TryCreate(href, UriKind.Absolute, out var absolute) && absolute.Scheme != "file")
                    continue;
                var target = new Uri(new Uri("https://audit.invalid" + key.Route), href);
                var path = Uri.UnescapeDataString(target.AbsolutePath);
                // Class references come from separately synchronized engine XML, not this documentation corpus.
                if (path.Split('/').Contains("Classes") || !path.StartsWith("/en/"))
                    continue;
                var resolved = paths.ResolveRoute(path[4..], versions);
                path = resolved?.PublicUrl ?? path;
                if (!pages.TryGetValue((key.Version, path), out var destination))
                    destination = pages.FirstOrDefault(p => p.Key.Route == path).Value;
                var fragment = Uri.UnescapeDataString(target.Fragment.TrimStart('#'));
                if (destination == null || fragment.Length > 0 &&
                    !destination.DocumentNode.Descendants().Any(n => n.GetAttributeValue("id", "") == fragment))
                    errors.Add(key.Version + ": " + key.Route + " -> " + href);
            }
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void DeepHeadingsHaveUniqueAnchorsWithoutExpandingTheTableOfContents()
    {
        var toc = new List<Redot_Documentation.Components.Layout.DocumentHeading>();
        var html = DocumentHeadings.Apply("<h2>Self</h2><h4>Self</h4><h5>Details</h5>", toc);
        Assert.Contains("id=\"self-2\"", html);
        Assert.Contains("id=\"details\"", html);
        Assert.Single(toc);
    }

    private sealed class AuditEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "DocumentationLinkTests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = "";
        public string WebRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
