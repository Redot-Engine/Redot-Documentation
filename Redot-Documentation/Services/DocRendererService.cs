namespace Redot_Documentation.Services;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Markdig;
using Redot_Documentation.Versioning;

public class DocRendererService
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly string docsRootPath;

    public DocRendererService(IWebHostEnvironment webHostEnvironment)
    {
        docsRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "docs");
    }

    public async Task<string> RenderToHtmlAsync(string documentPath, VersionProvider versionProvider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            throw new ArgumentException("Document path cannot be null or empty.", nameof(documentPath));
        }

        var normalizedPath = documentPath.Replace('\\', '/').Trim('/');
        var fullPath = Path.GetFullPath(Path.Combine(docsRootPath, normalizedPath));

        if (!fullPath.StartsWith(docsRootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Document path points outside the docs directory.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Markdown document not found: {documentPath}", fullPath);
        }

        var markdown = await File.ReadAllTextAsync(fullPath, cancellationToken);
        var transformedMarkdown = TransformMarkdown(markdown, versionProvider);
        var renderedHtml = Markdown.ToHtml(transformedMarkdown.Markdown, MarkdownPipeline);

        var placeholders = transformedMarkdown.HtmlPlaceholders;
        for (var pass = 0; pass < placeholders.Count; pass++)
        {
            var replacedInPass = false;

            foreach (var placeholder in placeholders)
            {
                if (!renderedHtml.Contains(placeholder.Key, StringComparison.Ordinal))
                {
                    continue;
                }

                renderedHtml = renderedHtml.Replace(placeholder.Key, placeholder.Value, StringComparison.Ordinal);
                replacedInPass = true;
            }

            if (!replacedInPass)
            {
                break;
            }
        }

        return renderedHtml;
    }

    private static (string Markdown, Dictionary<string, string> HtmlPlaceholders) TransformMarkdown(string markdown, VersionProvider versionProvider)
    {
        var transformedMarkdown = markdown;
        var htmlPlaceholders = new Dictionary<string, string>();

        // Transform Links
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @"\[[^\]]*\]\([^)]+\)",
            match => TransformLink(match.Value, versionProvider),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Parse callout blocks
        // tip
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @":::tip\s*\r?\n(.*?)\r?\n:::",
            match => TransformCallout(match.Value, "tip", htmlPlaceholders, IconConstants.TipsIcon),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        // note
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @":::note\s*\r?\n(.*?)\r?\n:::",
            match => TransformCallout(match.Value, "note", htmlPlaceholders, IconConstants.TipsIcon),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        // info
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @":::info\s*\r?\n(.*?)\r?\n:::",
            match => TransformCallout(match.Value, "info", htmlPlaceholders, IconConstants.TipsIcon),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        transformedMarkdown = TransformTabsBlocks(transformedMarkdown, htmlPlaceholders);
        // warning
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @":::warning\s*\r?\n(.*?)\r?\n:::",
            match => TransformCallout(match.Value, "warning", htmlPlaceholders, IconConstants.TipsIcon),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        transformedMarkdown = TransformTabsBlocks(transformedMarkdown, htmlPlaceholders);
        // Render tabs
        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @"^import\s+Tabs\s+from\s+[\""'][^\""']+[\""'];\s*$",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        transformedMarkdown = Regex.Replace(
            transformedMarkdown,
            @"^import\s+TabItem\s+from\s+[\""'][^\""']+[\""'];\s*$",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        return (transformedMarkdown, htmlPlaceholders);
    }

    private static string TransformLink(string matchValue, VersionProvider versionProvider)
    {
        var (linkName, linkUrl) = SplitMarkdownLink(matchValue);

        if (linkUrl.StartsWithAny(VersionProvider.SlugPrefixes))
        {
            try
            {
                linkUrl = versionProvider.GetPathFromSlug(linkUrl);
            }
            catch (KeyNotFoundException)
            {
                // Keep original URL when no slug mapping exists.
                Console.WriteLine($"No slug mapping found for {linkUrl}");
            }
        }

        if (linkName.StartsWith("doc_"))
        {
            linkName = linkName.Replace("doc_", "").Replace('_', ' ').CapitalizeEachWord();
        }
        linkName = linkName.Replace("_", " ");
        linkName = linkName.Trim();
        return $"[{linkName}]({linkUrl})";
    }

    /// <summary>
    /// Parses a markdown link in the format [name](url) and splits it into its constituent name and URL components.
    /// </summary>
    /// <param name="matchValue">The markdown link string in the format [name](url) to be parsed and split.</param>
    /// <returns>
    /// A tuple containing the name and URL as strings.
    /// The first item in the tuple is the name, and the second item is the URL.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the provided string does not match the expected markdown link format.</exception>
    private static (string name, string url) SplitMarkdownLink(string matchValue)
    {
        // normalize just in case.
        matchValue = matchValue.Trim();
        if (!matchValue.StartsWith("[") || !matchValue.EndsWith(")"))
            throw new ArgumentException("Invalid markdown link format");
        int endNameIndex = matchValue.IndexOf(']');
        if (endNameIndex == -1)
            throw new ArgumentException("Invalid markdown link format");
        if (matchValue[endNameIndex + 1] != '(')
            throw new ArgumentException("Invalid markdown link format");
        var name = matchValue.Substring(1, endNameIndex - 1);
        var url = matchValue.Substring(endNameIndex + 2, matchValue.Length - endNameIndex - 3);
        return (name, url);
    }

    private static string TransformTabsBlocks(string markdown, Dictionary<string, string> htmlPlaceholders)
    {
        var tabsIndex = 0;
        return Regex.Replace(
            markdown,
            @"<Tabs>(.*?)</Tabs>",
            match => TransformSingleTabsBlock(match, tabsIndex++, htmlPlaceholders),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private static string TransformSingleTabsBlock(Match tabsBlockMatch, int tabsIndex, Dictionary<string, string> htmlPlaceholders)
    {
        var tabsContent = tabsBlockMatch.Groups[1].Value;
        var tabItemMatches = Regex.Matches(
            tabsContent,
            @"<TabItem\b([^>]*)>(.*?)</TabItem>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (tabItemMatches.Count == 0)
        {
            return tabsBlockMatch.Value;
        }

        var tabButtonsMarkup = new List<string>(tabItemMatches.Count);
        var tabPanelsMarkup = new List<string>(tabItemMatches.Count);

        for (var i = 0; i < tabItemMatches.Count; i++)
        {
            var tabItemMatch = tabItemMatches[i];
            var attributes = tabItemMatch.Groups[1].Value;
            var body = tabItemMatch.Groups[2].Value.Trim();

            var label = GetAttributeValue(attributes, "label");
            var value = GetAttributeValue(attributes, "value");

            if (string.IsNullOrWhiteSpace(label))
            {
                label = !string.IsNullOrWhiteSpace(value) ? value : $"Tab {i + 1}";
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = $"tab-{i + 1}";
            }

            var panelId = $"doc-tab-{tabsIndex}-{SanitizeIdentifier(value)}";
            var isActive = i == 0;
            var activeClass = isActive ? " active" : string.Empty;

            tabButtonsMarkup.Add(
                $"<button class=\"doc-tab-button{activeClass}\" type=\"button\" role=\"tab\" data-tab-target=\"#{panelId}\" aria-selected=\"{isActive.ToString().ToLowerInvariant()}\">{WebUtility.HtmlEncode(label)}</button>");

            var panelHtml = Markdown.ToHtml(body, MarkdownPipeline).Trim();

            tabPanelsMarkup.Add(
                $"<div id=\"{panelId}\" class=\"doc-tab-panel{activeClass}\" role=\"tabpanel\" data-value=\"{WebUtility.HtmlEncode(value)}\" data-label=\"{WebUtility.HtmlEncode(label)}\">{panelHtml}</div>");
        }

        var tabsMarkup =
            "<div class=\"doc-tabs\">" +
            $"<div class=\"doc-tab-buttons\" role=\"tablist\">{string.Join(string.Empty, tabButtonsMarkup)}</div>" +
            $"<div class=\"doc-tab-panels\">{string.Join(string.Empty, tabPanelsMarkup)}</div>" +
            "</div>";

        return StoreHtmlBlock(htmlPlaceholders, tabsMarkup);
    }

    private static string StoreHtmlBlock(Dictionary<string, string> htmlPlaceholders, string html)
    {
        string placeholder;
        do
        {
            placeholder = $"<!--DOC_HTML_BLOCK_{System.Guid.NewGuid():N}-->";
        } while (htmlPlaceholders.ContainsKey(placeholder) || html.Contains(placeholder, StringComparison.Ordinal));

        htmlPlaceholders[placeholder] = html;
        return placeholder;
    }

    private static string GetAttributeValue(string attributes, string attributeName)
    {
        var match = Regex.Match(
            attributes,
            $@"\b{Regex.Escape(attributeName)}\s*=\s*[\""']([^\""']*)[\""']",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string SanitizeIdentifier(string value)
    {
        var sanitized = Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9_-]+", "-").Trim('-');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "tab";
        }

        return sanitized;
    }

    public static string TransformCallout(
        string markdownSection,
        string calloutType,
        Dictionary<string, string> htmlPlaceholders,
        string? iconPath = null)
    {
        string calloutLower = calloutType.ToLowerInvariant();
        string calloutUpper = calloutType.ToUpperInvariant();
        string transformedSection = markdownSection.Replace($":::{calloutLower}", string.Empty)
            .Replace(":::", string.Empty)
            .Trim();
        transformedSection = Markdown.ToHtml(transformedSection, MarkdownPipeline).Trim();
        string transformed = $"""
                              <div class="callout-{calloutLower}">
                                  <div class="callout-icon-{calloutLower}"></div>
                                  <div class="callout-content">
                                      <div class="callout-title">{calloutUpper}</div>
                                      <div class="callout-message">
                                              {transformedSection}
                                      </div>
                                  </div>
                              </div>
                              <br/>
                              """;
        return StoreHtmlBlock(htmlPlaceholders, transformed);
    }
}