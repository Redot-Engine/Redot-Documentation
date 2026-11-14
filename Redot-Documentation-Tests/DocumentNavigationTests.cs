using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redot_Documentation.Components.Layout;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class DocumentNavigationTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"redot-navigation-{Guid.NewGuid():N}");

    [Fact]
    public void NeighborsFollowSidebarOrderIncludingOverviewsNestedSectionsAndExclusions()
    {
        Write("About/index.md");
        Write("About/intro.md");
        Write("Contributing/index.md");
        Write("latest/index.json", """{"IntermingleArticles":true,"RankingPriorities":{"Guides":0,"Last.md":1}}""");
        Write("latest/Last.md");
        Write("latest/Guides/index.md");
        Write("latest/Guides/index.json", """{"RankingPriorities":{"z.md":0,"Nested":-1,"a.md":1},"ExcludedItems":["excluded.md"]}""");
        Write("latest/Guides/z.md");
        Write("latest/Guides/a.md");
        Write("latest/Guides/excluded.md");
        Write("latest/Guides/diagrams/ignored.md");
        Write("latest/Guides/Nested/index.md");
        Write("latest/Guides/Nested/child.md");
        var provider = Provider("latest");
        string[] expected = ["About/index.md", "About/intro.md", "Contributing/index.md",
            "latest/Guides/index.md", "latest/Guides/z.md", "latest/Guides/a.md",
            "latest/Guides/Nested/index.md", "latest/Guides/Nested/child.md", "latest/Last.md"];

        for (int i = 0; i < expected.Length; i++)
        {
            var neighbors = Neighbors(provider, expected[i]);
            Assert.Equal(i > 0 ? DocumentPathResolver.PublicUrl(expected[i - 1]) : null, neighbors.Previous?.Url);
            Assert.Equal(i + 1 < expected.Length ? DocumentPathResolver.PublicUrl(expected[i + 1]) : null, neighbors.Next?.Url);
        }
        Assert.Equal("Guides", Neighbors(provider, "Contributing/index.md").Next?.Title);
        Assert.Equal(DocumentNeighbors.Empty, Neighbors(provider, "latest/Guides/excluded.md"));
        Assert.Equal(DocumentNeighbors.Empty, Neighbors(provider, "latest/Guides/diagrams/ignored.md"));
        Assert.Equal(DocumentNeighbors.Empty, Neighbors(provider, "missing.md"));
    }

    [Theory]
    [InlineData("26.1")]
    [InlineData("latest")]
    public void SharedArticleLeadsIntoSelectedVersionsArticlesWithEscapedCanonicalUrls(string version)
    {
        Write("Contributing/last_shared.md");
        Write("26.1/Space ü.md");
        Write("latest/Space ü.md");
        var provider = Provider(version);
        Assert.Equal($"/en/{version}/Space%20%C3%BC", Neighbors(provider, "Contributing/last_shared.md").Next?.Url);
        Assert.Equal("/en/Contributing/last_shared", Neighbors(provider, $"{version}/Space ü.md").Previous?.Url);
        Assert.Null(Neighbors(provider, $"{version}/Space ü.md").Next);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task FooterRendersAccessibleLinksOnlyForExistingNeighbors(bool previous, bool next)
    {
        await using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        var neighbors = new DocumentNeighbors(previous ? new("Earlier & useful", "/en/latest/earlier") : null,
            next ? new("Later", "/en/latest/later") : null);
        string html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<ArticlePagination>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { ["Neighbors"] = neighbors }))).ToHtmlString());
        Assert.Equal(previous || next, html.Contains("aria-label=\"Article navigation\""));
        Assert.Equal(previous, html.Contains("rel=\"prev\""));
        Assert.Equal(next, html.Contains("rel=\"next\""));
        if (previous) Assert.Contains("Earlier &amp; useful", html);
        if (next) Assert.Contains("href=\"/en/latest/later\"", html);
    }

    private VersionProvider Provider(string version)
    {
        var provider = new VersionProvider(new DocumentationVersion
        { Slug = version, FriendlyName = version, BranchName = version }, root);
        provider.SortRankings();
        return provider;
    }

    private DocumentNeighbors Neighbors(VersionProvider provider, string path)
        => DocumentNavigation.GetNeighbors(provider, new(Path.Combine(root, path), path,
            DocumentPathResolver.PublicUrl(path)));

    private void Write(string path, string text = "# Article")
    {
        string fullPath = Path.Combine(root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, text);
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
