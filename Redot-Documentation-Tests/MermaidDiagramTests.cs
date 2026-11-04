using System.Text.Json;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Redot_Documentation.Search;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class MermaidDiagramTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "redot-mermaid-tests-" + Guid.NewGuid().ToString("N"));
    private readonly TestEnvironment environment;
    private readonly VersionManagerService versions;

    public MermaidDiagramTests()
    {
        environment = new TestEnvironment(root);
        Write("26.1/Tutorials/source.md", "# Source");
        Write("latest/Tutorials/source.md", "# Development");
        Write("About/shared.md", "# Shared");
        Write("Versions.json", JsonSerializer.Serialize(new[]
        {
            new DocumentationVersion { Slug = "26.1", FriendlyName = "Stable", BranchName = "stable", IsLatestStable = true },
            new DocumentationVersion { Slug = "latest", FriendlyName = "Development", BranchName = "master", IsNextPrerelease = true }
        }));
        versions = new VersionManagerService(environment);
        versions.LoadContent();
    }

    [Theory]
    [InlineData("<mermaid ref=\"./diagrams/Scene Lifecycle.mmd\" />")]
    [InlineData("<mermaid ref='./diagrams/Scene Lifecycle.mmd'></mermaid>")]
    [InlineData("<mermaid slug=\"scene-lifecycle\"/>")]
    [InlineData("<MERMAID\nSLUG = 'SCENE-LIFECYCLE' >\n</MERMAID>")]
    public async Task BothTagFormsAndSelectorsRenderAnImageWithoutConsumingFollowingContent(string embed)
    {
        WriteDiagram("26.1/Tutorials/diagrams/Scene Lifecycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", "# Before\n\n" + embed + "\n\n## After\n\nFollowing content.");
        var catalog = Catalog();
        Assert.Empty(catalog.Errors);
        var html = await Render(catalog);
        Assert.Contains("class=\"doc-mermaid\"", html);
        Assert.Contains("alt=\"Lifecycle\"", html);
        Assert.Contains("Following content.", html);
        Assert.Contains(">After</h2>", html);
        Assert.DoesNotContain("<mermaid", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DOC_MERMAID", html);
        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("flowchart", html);
    }

    [Fact]
    public async Task VersionsAndSharedPagesUseTheirOwnSlugsAndExplicitSharedPaths()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "StableOnly");
        WriteDiagram("latest/Tutorials/diagrams/cycle.mmd", "DevelopmentOnly");
        WriteDiagram("About/diagrams/cycle.mmd", "SharedOnly");
        var catalog = Catalog();
        var paths = new DocumentPathResolver(environment, versions);
        var stable = versions.GetVersionProvider("26.1");
        var development = versions.GetVersionProvider("latest");
        var stableDocument = paths.Resolve("Tutorials/source.md", stable)!;
        var developmentDocument = paths.Resolve("Tutorials/source.md", development)!;
        var sharedDocument = paths.Resolve("About/shared.md", stable)!;
        var stableDiagram = catalog.Resolve(stableDocument, stable, null, "cycle");
        var developmentDiagram = catalog.Resolve(developmentDocument, development, null, "cycle");
        var sharedDiagram = catalog.Resolve(sharedDocument, development, null, "cycle");
        Assert.NotEqual(stableDiagram.Key, developmentDiagram.Key);
        Assert.NotEqual(stableDiagram.Key, sharedDiagram.Key);
        Assert.Equal(stableDiagram, catalog.Resolve(stableDocument, stable, "/Tutorials/diagrams/CYCLE.MMD", null));
        Assert.Equal(sharedDiagram, catalog.Resolve(stableDocument, stable, "/About/diagrams/cycle.mmd", null));
        Assert.Equal(stableDiagram, catalog.Resolve(stableDocument, stable, "../Tutorials/diagrams/cycle.mmd", null));
        Assert.Contains("StableOnly", await Image(catalog, stableDiagram.Key));
        Assert.Contains("DevelopmentOnly", await Image(catalog, developmentDiagram.Key));
        Assert.Contains("SharedOnly", await Image(catalog, sharedDiagram.Key));
        Assert.Equal(stableDocument, paths.ResolveRoute("Tutorials/source", versions));
    }

    [Theory]
    [InlineData("../../latest/Tutorials/diagrams/cycle.mmd")]
    [InlineData("../../About/diagrams/cycle.mmd")]
    [InlineData("/latest/Tutorials/diagrams/cycle.mmd")]
    [InlineData("../../../../outside.mmd")]
    [InlineData("https://example.com/cycle.mmd")]
    [InlineData("//example.com/cycle.mmd")]
    [InlineData("./diagrams/cycle.mmd?version=latest")]
    [InlineData("./diagrams/cycle.mmd#fragment")]
    [InlineData("./diagrams/%2e%2e/cycle.mmd")]
    [InlineData(".\\diagrams\\cycle.mmd")]
    [InlineData("./source.md")]
    public async Task UnsafeAndCrossVersionReferencesProduceLocalDiagnostics(string reference)
    {
        WriteDiagram("latest/Tutorials/diagrams/cycle.mmd", "Development");
        WriteDiagram("About/diagrams/cycle.mmd", "Shared");
        Write("26.1/Tutorials/source.md", "# Page\n\n<mermaid ref=\"" + reference + "\"/>\n\nStill here.");
        var html = await Render(Catalog());
        Assert.Contains("doc-mermaid-error", html);
        Assert.Contains("Still here.", html);
        Assert.DoesNotContain(root, html);
        Assert.DoesNotContain("<img", html);
    }

    [Fact]
    public async Task AmbiguousSlugsRequirePathsAndMissingVersionsDoNotFallBack()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "First");
        WriteDiagram("26.1/Other/diagrams/Cycle.mmd", "Second");
        var catalog = Catalog();
        Write("26.1/Tutorials/source.md", "<mermaid slug='cycle'/>");
        Assert.Contains("ambiguous", await Render(catalog));
        Write("26.1/Tutorials/source.md", "<mermaid ref='./diagrams/cycle.mmd'/>");
        Assert.DoesNotContain("doc-mermaid-error", await Render(catalog));
        Write("latest/Tutorials/source.md", "<mermaid slug='cycle'/>");
        Assert.Contains("No diagram has slug", await Render(catalog, "latest"));
    }

    [Theory]
    [InlineData("<mermaid/>")]
    [InlineData("<mermaid ref='x.mmd' slug='cycle'/>")]
    [InlineData("<mermaid ref=''/>")]
    [InlineData("<mermaid slug='cycle' alt=' '/>")]
    [InlineData("<mermaid slug='cycle' SLUG='cycle'/>")]
    [InlineData("<mermaid slug=cycle/>")]
    [InlineData("<mermaid slug='cycle'alt='Missing space'/>")]
    [InlineData("<mermaid slug='cycle' onclick='evil()'/>")]
    [InlineData("<mermaid slug='cycle'>Unexpected body</mermaid>")]
    [InlineData("<mermaid slug='cycle'>")]
    [InlineData("<mermaid ref=\"missing.mmd />")]
    public async Task InvalidEmbedsAreExplainedWithoutBreakingThePage(string embed)
    {
        Write("26.1/Tutorials/source.md", embed + "\n\nFollowing text.");
        var html = await Render(Catalog());
        Assert.Contains("doc-mermaid-error", html);
        Assert.Contains("Following text.", html);
    }

    [Theory]
    [InlineData("`<mermaid slug='missing'/>`")]
    [InlineData("\\<mermaid slug='missing'/>")]
    [InlineData("<mermaid-extra slug='missing'/>")]
    [InlineData("```html\n<mermaid slug='missing'/>\n```")]
    [InlineData("    <mermaid slug='missing'/>")]
    [InlineData("<!-- <mermaid slug='missing'/> -->")]
    [InlineData("<pre><code><mermaid slug='missing'/></code></pre>")]
    [InlineData("<Tabs>\n<TabItem label='Example'>\n\n```html\n<mermaid slug='missing'/>\n```\n\n</TabItem>\n</Tabs>")]
    [InlineData(":::note\n\n```html\n<mermaid slug='missing'/>\n```\n\n:::")]
    public async Task LiteralExamplesAreNotResolved(string markdown)
    {
        Write("26.1/Tutorials/source.md", markdown);
        var html = await Render(Catalog());
        Assert.DoesNotContain("doc-mermaid-error", html);
        Assert.DoesNotContain("class=\"doc-mermaid\"", html);
        Assert.Contains("missing", html);
    }

    [Theory]
    [InlineData("<span title=\"<mermaid slug='missing'/>\">Example</span>")]
    [InlineData("<div data-example='<mermaid ref=\"./missing.mmd\"/>'>Example</div>")]
    [InlineData("<div title=\"5 > 3\" data-example=\"<mermaid slug='missing'></mermaid>\">Example</div>")]
    [InlineData("<div title=\"Example\n<mermaid slug='missing'/>\">Example</div>")]
    public async Task MermaidTextInsideHtmlAttributesIsNotExpanded(string literal)
    {
        var placeholders = new Dictionary<string, string>();
        var unchanged = MermaidEmbedTransform.Extract(literal,
            _ => throw new InvalidOperationException("An HTML attribute must not render a Mermaid embed."), placeholders);
        Assert.Equal(literal, unchanged);
        Assert.Empty(placeholders);

        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", literal + "\n\n<mermaid slug='cycle'/>");
        var html = await Render(Catalog());
        var document = new HtmlDocument();
        document.LoadHtml(html);
        Assert.Single(document.DocumentNode.SelectNodes("//figure[@class='doc-mermaid']//img") ?? Enumerable.Empty<HtmlNode>());
        Assert.DoesNotContain("doc-mermaid-error", html);
    }

    [Theory]
    [InlineData("code")]
    [InlineData("pre")]
    [InlineData("script")]
    [InlineData("style")]
    public async Task EscapedHtmlOpeningsCannotSuppressRealEmbeds(string tag)
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", $"""
            \<{tag}>

            <mermaid slug='cycle'/>

            </{tag}>
            """);
        var html = await Render(Catalog());
        Assert.Contains("<img", html);
        Assert.DoesNotContain("doc-mermaid-error", html);
    }

    [Theory]
    [InlineData("```html\n<span title=\"\n```", "```html\n\">\n```")]
    [InlineData("\\<div title=\"", "\">")]
    [InlineData("<!-- <span title=\" -->", "<!-- \"> -->")]
    [InlineData("<script>const text = '<span title=\"';</script>", "\">")]
    [InlineData("<style>/* <span title=\" */</style>", "\">")]
    [InlineData("<pre><span title=\"</pre>", "\">")]
    [InlineData("<code><span title=\"</code>", "\">")]
    public void LiteralHtmlExamplesCannotCreateAttributeSpansAcrossMarkdown(string before, string after)
    {
        var markdown = before + "\n\n`<mermaid slug='missing'/>`\n\n<mermaid slug='cycle'/>\n\n" + after;
        var placeholders = new Dictionary<string, string>();
        var extracted = MermaidEmbedTransform.Extract(markdown, attributes =>
        {
            Assert.Equal("cycle", attributes["slug"]);
            return "<figure>Ready</figure>";
        }, placeholders);
        Assert.Single(placeholders);
        Assert.Contains("`<mermaid slug='missing'/>`", extracted);
        Assert.DoesNotContain("<mermaid slug='cycle'/>", extracted);
    }

    [Fact]
    public async Task EmbedsInsideTabsAndCalloutsSurviveAllPlaceholderPasses()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", """
            <Tabs>
            <TabItem label="Diagram">

            :::note

            <mermaid slug="cycle"/>

            :::

            </TabItem>
            </Tabs>
            """);
        var html = await Render(Catalog());
        var document = new HtmlDocument();
        document.LoadHtml(html);
        Assert.NotNull(document.DocumentNode.SelectSingleNode("//div[contains(@class,'doc-tab-panel')]//div[@class='callout-message']//figure[@class='doc-mermaid']//img"));
        Assert.DoesNotContain("DOC_", html);
    }

    [Fact]
    public async Task UnclosedTagsCannotConsumeArticleContentOrClosingTagsInExamples()
    {
        Write("26.1/Tutorials/source.md", """
            <mermaid slug='missing'>

            ## Important article text

            ```html
            </mermaid>
            ```

            After.
            """);
        var html = await Render(Catalog());
        Assert.Contains("doc-mermaid-error", html);
        Assert.Contains(">Important article text</h2>", html);
        Assert.Contains("&lt;/mermaid&gt;", html);
        Assert.Contains("After.", html);
    }

    [Fact]
    public async Task HtmlExamplesCannotSuppressSubsequentRealEmbeds()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", """
            ```html
            <code>
            ```

            <mermaid slug='cycle'/>

            <pre><mermaid slug='missing'/></pre>

            ```html
            </code>
            ```
            """);
        var html = await Render(Catalog());
        Assert.Contains("<img", html);
        Assert.DoesNotContain("doc-mermaid-error", html);
    }

    [Theory]
    [InlineData("<code>", "</code>")]
    [InlineData("A `quoted", "label` B")]
    public async Task PlainTextAttributesCannotCreateLiteralSpansAcrossOtherEmbeds(string first, string second)
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/source.md", $"<mermaid slug='cycle' caption='{first}'/>\n\n<mermaid slug='cycle' caption='{second}'/>");
        var html = await Render(Catalog());
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var images = document.DocumentNode.SelectNodes("//figure[@class='doc-mermaid']//img");
        Assert.NotNull(images);
        Assert.Equal(2, images.Count);
        Assert.DoesNotContain("doc-mermaid-error", html);
    }

    [Fact]
    public async Task AttributesDecodeEntitiesAndEncodeOutputWhileSearchUsesCaptions()
    {
        WriteDiagram("26.1/Tutorials/diagrams/A & B.mmd", "RawDiagramLabel");
        Write("26.1/Tutorials/source.md", """
            # Page

            <mermaid ref='./diagrams/A &amp; B.mmd' alt='5 > 3 &amp; &quot;quoted&quot;' caption='&lt;script&gt;Caption&lt;/script&gt;' />

            <mermaid slug='missing'/>
            """);
        var html = await Render(Catalog());
        Assert.Contains("alt=\"5 &gt; 3 &amp; &quot;quoted&quot;\"", html);
        Assert.Contains("&lt;script&gt;Caption&lt;/script&gt;", html);
        var text = string.Join(" ", SearchContent.Extract(html, "/source", "guides").Select(section => section.Body));
        Assert.Contains("Caption", text);
        Assert.DoesNotContain("Diagram unavailable", text);
        Assert.DoesNotContain("RawDiagramLabel", text);
    }

    [Theory]
    [InlineData("flowchart LR\n subgraph Group\n A[Start] --> B[Finish]\n end")]
    [InlineData("sequenceDiagram\n participant A as Writer\n participant B as Reader\n A->>B: Hello")]
    [InlineData("stateDiagram-v2\n [*] --> Idle\n Idle --> Running\n Running --> [*]")]
    public void RepresentativeDiagramTypesRenderValidSvg(string source)
    {
        Write("26.1/Tutorials/diagrams/example.mmd", source);
        var catalog = Catalog();
        Assert.Empty(catalog.Errors);
        Assert.Single(Directory.GetFiles(Path.Combine(root, "App_Data", "mermaid"), "*.svg"));
    }

    [Fact]
    public async Task InvalidSourcesDoNotPreventOtherDiagramsAndPagesFromLoading()
    {
        Write("26.1/Tutorials/diagrams/broken.mmd", "not a diagram");
        WriteDiagram("26.1/Tutorials/diagrams/valid.mmd", "StillWorks");
        Write("26.1/Tutorials/source.md", "<mermaid slug='broken'/>\n\n<mermaid slug='valid'/>");
        var catalog = Catalog();
        Assert.Single(catalog.Errors);
        var html = await Render(catalog);
        Assert.Contains("doc-mermaid-error", html);
        Assert.Contains("<img", html);
    }

    [Fact]
    public void CachedOutputsAreReusedRepairedAndInvalidatedBySourceChanges()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "First");
        var catalog = Catalog();
        Assert.Empty(catalog.Errors);
        var fingerprint = catalog.Fingerprint("26.1");
        var cached = Assert.Single(Directory.GetFiles(Path.Combine(root, "App_Data", "mermaid"), "*.svg"));
        var timestamp = DateTime.UtcNow.AddDays(-1);
        File.SetLastWriteTimeUtc(cached, timestamp);
        Assert.Empty(Catalog().Errors);
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(cached));
        File.WriteAllText(cached, "<svg xmlns='http://www.w3.org/2000/svg'><script>evil()</script></svg>");
        Assert.Empty(Catalog().Errors);
        Assert.DoesNotContain("<script", File.ReadAllText(cached));
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Second");
        Assert.NotEqual(fingerprint, Catalog().Fingerprint("26.1"));
    }

    [Fact]
    public async Task AnUnavailablePersistentCacheDoesNotPreventRendering()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        File.WriteAllText(Path.Combine(root, "App_Data"), "This path is not a directory.");
        Write("26.1/Tutorials/source.md", "<mermaid slug='cycle'/>");
        var catalog = Catalog();
        Assert.Empty(catalog.Errors);
        Assert.Contains("<img", await Render(catalog));
    }

    [Fact]
    public async Task AnUnreadableCacheEntryIsRegeneratedInMemory()
    {
        if (OperatingSystem.IsWindows()) return;
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        Assert.Empty(Catalog().Errors);
        var cached = Assert.Single(Directory.GetFiles(Path.Combine(root, "App_Data", "mermaid"), "*.svg"));
        try
        {
            File.SetUnixFileMode(cached, UnixFileMode.None);
            try
            {
                File.ReadAllBytes(cached);
                return; // This account can bypass the mode, so the unreadable-cache path cannot be tested.
            }
            catch (UnauthorizedAccessException) { }
            Write("26.1/Tutorials/source.md", "<mermaid slug='cycle'/>");
            var catalog = Catalog();
            Assert.Empty(catalog.Errors);
            Assert.Contains("<img", await Render(catalog));
        }
        finally { File.SetUnixFileMode(cached, UnixFileMode.UserRead | UnixFileMode.UserWrite); }
    }

    [Fact]
    public async Task SvgEndpointSetsCacheAndIsolationHeadersAndHonorsEtags()
    {
        WriteDiagram("26.1/Tutorials/diagrams/cycle.mmd", "Ready");
        var catalog = Catalog();
        var paths = new DocumentPathResolver(environment, versions);
        var provider = versions.GetVersionProvider("26.1");
        var diagram = catalog.Resolve(paths.Resolve("Tutorials/source", provider)!, provider, null, "cycle");
        var context = Context();
        await catalog.Image(diagram.Key, context).ExecuteAsync(context);
        var body = ((MemoryStream)context.Response.Body).ToArray();
        Assert.Equal(diagram.Key, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(body)).ToLowerInvariant());
        Assert.Equal("image/svg+xml", context.Response.ContentType);
        Assert.Contains("immutable", context.Response.Headers.CacheControl.ToString());
        Assert.Contains("sandbox", context.Response.Headers.ContentSecurityPolicy.ToString());
        Assert.Equal("nosniff", context.Response.Headers.XContentTypeOptions);
        var conditional = Context();
        conditional.Request.Headers.IfNoneMatch = context.Response.Headers.ETag;
        await catalog.Image(diagram.Key, conditional).ExecuteAsync(conditional);
        Assert.Equal(304, conditional.Response.StatusCode);
        var missing = Context();
        await catalog.Image("../private", missing).ExecuteAsync(missing);
        Assert.Equal(404, missing.Response.StatusCode);
    }

    [Fact]
    public async Task SymbolicLinksAndUnconfiguredVersionsAreNotCatalogued()
    {
        WriteDiagram("unconfigured/diagrams/cycle.mmd", "OtherVersion");
        WriteDiagram("About/diagrams/real.mmd", "Shared");
        Directory.CreateDirectory(Path.Combine(root, "docs", "26.1", "Tutorials", "diagrams"));
        try
        {
            File.CreateSymbolicLink(Path.Combine(root, "docs", "26.1", "Tutorials", "diagrams", "cycle.mmd"), Path.Combine(root, "docs", "About", "diagrams", "real.mmd"));
            Directory.CreateSymbolicLink(Path.Combine(root, "docs", "26.1", "Tutorials", "linked"), Path.Combine(root, "docs", "About", "diagrams"));
        }
        catch (Exception exception) when (OperatingSystem.IsWindows() && exception is IOException or UnauthorizedAccessException)
        {
            // Windows may lack symlink privileges; the unconfigured-version check still runs.
        }
        Write("26.1/Tutorials/source.md", "<mermaid slug='cycle'/>\n\n<mermaid ref='./linked/real.mmd'/>");
        var html = await Render(Catalog());
        Assert.DoesNotContain("<img", html);
    }

    [Fact]
    public void DiagramFoldersDoNotBecomeNavigationSectionsEvenWithExplicitConfiguration()
    {
        WriteDiagram("26.1/Tutorials/Diagrams/cycle.mmd", "Ready");
        Write("26.1/Tutorials/index.json", "{}");
        versions.LoadContent();
        var tutorial = Assert.Single(versions.GetVersionProvider("26.1").VersionedDocsSection!.SubSections);
        Assert.Empty(tutorial.SubSections);
        Assert.Single(tutorial.Articles);
    }

    private MermaidDiagramCatalog Catalog() => new(environment, versions, NullLogger<MermaidDiagramCatalog>.Instance);

    private Task<string> Render(MermaidDiagramCatalog catalog, string version = "26.1")
        => new DocRendererService(new DocumentPathResolver(environment, versions), catalog)
            .RenderToHtmlAsync("Tutorials/source.md", versions.GetVersionProvider(version));

    private static DefaultHttpContext Context()
    {
        var context = new DefaultHttpContext { RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider() };
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> Image(MermaidDiagramCatalog catalog, string key)
    {
        var context = Context();
        await catalog.Image(key, context).ExecuteAsync(context);
        var bytes = ((MemoryStream)context.Response.Body).ToArray();
        var document = XDocument.Parse(System.Text.Encoding.UTF8.GetString(bytes));
        Assert.Equal("svg", document.Root!.Name.LocalName);
        return document.ToString();
    }

    private void WriteDiagram(string relative, string label)
        => Write(relative, "flowchart LR\n accTitle: Lifecycle\n A[Start] --> B[" + label + "]");

    private void Write(string relative, string content)
    {
        var path = Path.Combine(root, "docs", relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = root;
        public string ContentRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
