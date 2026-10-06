// File: Redot-Documentation-Tests/DocumentationLinkTests.cs
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Redot_Documentation.Services;

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
        var renderer = new DocRendererService(paths);
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
                var target = new Uri(new Uri("http://localhost" + key.Route), href);
                var route = target.AbsolutePath;
                if (!pages.ContainsKey((key.Version, route)) && !pages.Keys.Any(p => p.Route.Equals(route, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Broken link in version {key.Version} at {key.Route}: {href} (resolved to {route})");
                }
            }
        Assert.Empty(errors);
    }
}
