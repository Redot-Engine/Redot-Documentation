using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Components.Layout;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class ClassDocumentationRendererTests
{
    [Fact]
    public void RenderMarkup_RendersKnownMarkupAndEncodesUntrustedHtml()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();
        ClassDocumentationEntry node = snapshot.Classes["Node"];

        string html = renderer.RenderMarkup(
            "Use [method Object.free], [Node], [param child], [b]carefully[/b], and [code]<unsafe>[/code]. <script>alert(1)</script>",
            node,
            snapshot);

        Assert.Contains("href=\"/en/latest/Classes/Object#method-free\"", html);
        Assert.Contains("href=\"/en/latest/Classes/Node\"", html);
        Assert.Contains("<code>child</code>", html);
        Assert.Contains("<strong>carefully</strong>", html);
        Assert.Contains("<code>&lt;unsafe&gt;</code>", html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void RenderPage_RendersSignaturesStatusesAndStableAnchors()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();

        string html = renderer.RenderPage(snapshot.Classes["Node"], snapshot);

        Assert.Contains("<h1>Node</h1>", html);
        Assert.Contains("id=\"method-add-child\"", html);
        Assert.Contains("href=\"/en/latest/Classes/Object\"", html);
        Assert.Contains("class-status-experimental", html);
        Assert.Contains("<span class=\"token class-name\">Node</span></a> <span class=\"token variable\">child</span>", html);
    }

    [Fact]
    public void RenderPage_HighlightsSignaturesAndCollectsMatchingMemberHeadings()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();
        ClassDocumentationEntry node = snapshot.Classes["Node"] with
        {
            Description = "A useful class.",
            Members = [new ClassDocumentationMember { Name = "enabled", Type = "bool", Default = "true" }],
            Methods =
            [
                new ClassDocumentationCallable
                {
                    Name = "add_child", ReturnType = "void", Qualifiers = "virtual const",
                    Parameters = [new ClassDocumentationParameter("child", "Node", "Node(1, 2)", null, false)]
                },
                new ClassDocumentationCallable
                {
                    Name = "add_child", ReturnType = "void",
                    Parameters = [new ClassDocumentationParameter("unsafe", "String", "\"<script>\"", null, false)]
                }
            ],
            Constants = [new ClassDocumentationConstant { Name = "MAX_COUNT", Value = "7" }]
        };
        var headings = new List<DocumentHeading>();

        string html = renderer.RenderPage(node, snapshot, headings);
        var document = new HtmlDocument();
        document.LoadHtml(html);

        Assert.Equal(
            [
                new(2, "description", "Description"),
                new(2, "properties", "Properties"),
                new(3, "member-enabled", "enabled"),
                new(2, "methods", "Methods"),
                new(3, "method-add-child", "add_child"),
                new(3, "method-add-child-2", "add_child (overload 2)"),
                new(2, "constants", "Constants"),
                new(3, "constant-max-count", "MAX_COUNT")
            ], headings);
        Assert.Equal(headings.Select(heading => heading.Id),
            document.DocumentNode.SelectNodes("//h2[@id]|//h3[@id]")!.Select(node => node.GetAttributeValue("id", "")));
        Assert.Contains("<span class=\"token boolean\">true</span>", html);
        Assert.Contains("<span class=\"token keyword\">virtual const</span>", html);
        Assert.Contains("<span class=\"token function\">Node</span>", html);
        Assert.Contains("<span class=\"token number\">7</span>", html);
        Assert.Contains("href=\"/en/latest/Classes/Node\"><span class=\"token class-name\">Node</span></a>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("<code><a", html);
    }

    [Fact]
    public void RenderMarkup_RendersLanguageCodeBlocksAsTabsWithoutInvalidParagraphs()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();

        string html = renderer.RenderMarkup(
            """
            Example:

            [codeblocks]
            [gdscript]
            print("hello")
            [/gdscript]
            [csharp]
            GD.Print("hello");
            [/csharp]
            [/codeblocks]
            """,
            snapshot.Classes["Node"],
            snapshot);

        Assert.Contains("class=\"doc-tabs\"", html);
        Assert.Contains("GDScript", html);
        Assert.Contains("C#", html);
        Assert.DoesNotContain("<p><div class=\"doc-tabs\"", html);
    }

    [Fact]
    public void RenderPage_GivesIdenticalTabbedCodeBlocksUniquePanelIds()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();
        const string markup = "[codeblocks][gdscript]print(1)[/gdscript][csharp]Print(1);[/csharp][/codeblocks]";
        ClassDocumentationEntry node = snapshot.Classes["Node"] with
        {
            BriefDescription = markup,
            Description = markup
        };
        var classes = new Dictionary<string, ClassDocumentationEntry>(snapshot.Classes, StringComparer.OrdinalIgnoreCase)
        {
            [node.Name] = node
        };

        string html = renderer.RenderPage(node, snapshot with { Classes = classes });

        string[] panelIds = Regex.Matches(html, "<div id=\"(?<id>class-doc-tab-[^\"]+)\"")
            .Select(match => match.Groups["id"].Value)
            .ToArray();
        string[] targets = Regex.Matches(html, "data-tab-target=\"#(?<id>class-doc-tab-[^\"]+)\"")
            .Select(match => match.Groups["id"].Value)
            .ToArray();

        Assert.Equal(4, panelIds.Length);
        Assert.Equal(panelIds.Length, panelIds.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(panelIds.Order(), targets.Order());
        TabMarkupAssertions.AssertAccessibleTabs(html, 4);
    }

    [Fact]
    public void RenderMarkup_RejectsProtocolRelativeUrlsAndAllowsSiteRelativeUrls()
    {
        var renderer = new ClassDocumentationRenderer();
        ClassDocumentationSnapshot snapshot = CreateSnapshot();

        string html = renderer.RenderMarkup(
            "[url=//evil.example/path]Untrusted[/url] [url=/safe/path]Safe[/url]",
            snapshot.Classes["Node"],
            snapshot);

        Assert.DoesNotContain("href=\"//evil.example/path\"", html);
        Assert.Contains("Untrusted", html);
        Assert.Contains("href=\"/safe/path\"", html);
    }

    private static ClassDocumentationSnapshot CreateSnapshot()
    {
        var node = new ClassDocumentationEntry
        {
            Name = "Node",
            Inherits = "Object",
            BriefDescription = "A node.",
            Experimental = "Use carefully.",
            Methods =
            [
                new ClassDocumentationCallable
                {
                    Name = "add_child",
                    Parameters = [new ClassDocumentationParameter("child", "Node", null, null, false)]
                }
            ]
        };
        var objectEntry = new ClassDocumentationEntry { Name = "Object" };
        var classes = new Dictionary<string, ClassDocumentationEntry>(StringComparer.OrdinalIgnoreCase)
        {
            [node.Name] = node,
            [objectEntry.Name] = objectEntry
        };
        return new ClassDocumentationSnapshot(
            new DocumentationVersion
            {
                Slug = "latest",
                FriendlyName = "Latest development",
                BranchName = "master",
                IsNextPrerelease = true
            },
            "1234567890abcdef",
            DateTimeOffset.UtcNow,
            classes);
    }
}
