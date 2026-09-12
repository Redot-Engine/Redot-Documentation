using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class DocRendererServiceTests : IDisposable
{
    private readonly string contentRootPath = Path.Combine(
        Path.GetTempPath(),
        $"redot-documentation-tests-{Guid.NewGuid():N}");

    [Theory]
    [InlineData("doc_some_doc#some_section", "/en/About/some_doc.md#some-section")]
    [InlineData("doc_some_doc#some-section", "/en/About/some_doc.md#some-section")]
    [InlineData("doc_some_doc#Some%20Section", "/en/About/some_doc.md#some-section")]
    [InlineData("doc_some_doc", "/en/About/some_doc.md")]
    public async Task RenderToHtmlAsync_ResolvesDocumentSlugsWithOptionalSections(
        string target,
        string expectedHref)
    {
        Directory.CreateDirectory(Path.Combine(contentRootPath, "docs"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRootPath, "docs", "source.md"),
            $"[Some Section]({target})");

        var renderer = new DocRendererService(new TestWebHostEnvironment(contentRootPath));

        var html = await renderer.RenderToHtmlAsync("source.md", CreateVersionProvider());

        Assert.Contains($"href=\"{expectedHref}\"", html);
    }

    [Fact]
    public async Task RenderToHtmlAsync_PreservesUnknownDocumentSlugAndSection()
    {
        Directory.CreateDirectory(Path.Combine(contentRootPath, "docs"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRootPath, "docs", "source.md"),
            "[Missing](doc_missing#some_section)");

        var renderer = new DocRendererService(new TestWebHostEnvironment(contentRootPath));

        var html = await renderer.RenderToHtmlAsync("source.md", CreateVersionProvider());

        Assert.Contains("href=\"doc_missing#some_section\"", html);
    }

    [Theory]
    [InlineData("class_Node", "/en/latest/Classes/Node")]
    [InlineData("class_Node#description", "/en/latest/Classes/Node#description")]
    [InlineData("class_Node_method_add_child#description", "/en/latest/Classes/Node#method-add-child")]
    [InlineData("class_Node_property_name#Some%20Section", "/en/latest/Classes/Node#member-name")]
    [InlineData("class_Node_method_add_child", "/en/latest/Classes/Node#method-add-child")]
    [InlineData("class_Node_property_name", "/en/latest/Classes/Node#member-name")]
    [InlineData("class_Node_method_get_annotation_list", "/en/latest/Classes/Node#method-get-annotation-list")]
    public async Task RenderToHtmlAsync_ResolvesClassReferenceSlugs(string target, string expectedHref)
    {
        Directory.CreateDirectory(Path.Combine(contentRootPath, "docs"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRootPath, "docs", "source.md"),
            $"[Class reference]({target})");

        var renderer = new DocRendererService(new TestWebHostEnvironment(contentRootPath));

        string html = await renderer.RenderToHtmlAsync("source.md", CreateVersionProvider());

        Assert.Contains($"href=\"{expectedHref}\"", html);
    }

    [Theory]
    [InlineData("Some Section", "some_section")]
    [InlineData("Some_Section", "some%5Fsection")]
    [InlineData("The interactive rebase", "the_interactive_rebase")]
    [InlineData("Pre-commit hook", "pre-commit-hook")]
    public void HeadingAnchor_UsesTheSameIdForTitlesAndSectionReferences(
        string title,
        string reference)
    {
        Assert.Equal(HeadingAnchor.FromTitle(title), HeadingAnchor.FromReference(reference));
    }

    public void Dispose()
    {
        if (Directory.Exists(contentRootPath))
        {
            Directory.Delete(contentRootPath, recursive: true);
        }
    }

    private VersionProvider CreateVersionProvider()
    {
        var about = new Section("About", "unused");
        about.Articles.Add(new Article(
            "some_doc.md",
            Path.Combine(contentRootPath, "docs", "About", "some_doc.md"),
            "doc_some_doc.md"));
        about.SortRankings();

        var community = new Section("Community", "unused");
        community.SortRankings();

        var contributing = new Section("Contributing", "unused");
        contributing.SortRankings();

        var provider = new VersionProvider(
            new DocumentationVersion
            {
                Slug = "latest",
                FriendlyName = "Latest development",
                BranchName = "master",
                IsNextPrerelease = true
            },
            Path.Combine(contentRootPath, "docs"))
        {
            AboutSection = about,
            CommunitySection = community,
            ContributingSection = contributing
        };
        provider.ParseSlugs();
        return provider;
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
