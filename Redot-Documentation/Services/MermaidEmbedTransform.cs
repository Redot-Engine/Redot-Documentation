using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Redot_Documentation.Services;

/// <summary>Extracts HTML-like Mermaid embeds before Markdown, tabs, and callouts are transformed.</summary>
internal static class MermaidEmbedTransform
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);
    // Disabling raw HTML here lets us recognize code examples inside TabItem and callout bodies too.
    private static readonly MarkdownPipeline LiteralPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
    private static readonly Regex Openings = new(@"<mermaid(?=[\s/>])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Tags = new("""\G<mermaid(?=[\s/>])(?<attributes>(?:\s+[a-zA-Z][a-zA-Z0-9_-]*\s*=\s*(?:"[^"\r\n]*"|'[^'\r\n]*'))*\s*)(?<self>/\s*)?>""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex HtmlMarkup = new("""(?<comment><!--[\s\S]*?-->)|<(?<name>[a-zA-Z][a-zA-Z0-9:-]*)(?=[\s/>])(?:[^"'<>]|"[^"]*"|'[^']*')*>""", RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Closing = new(@"\G\s*</mermaid\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Attribute = new("""\G\s+(?<name>[a-zA-Z][a-zA-Z0-9_-]*)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""", RegexOptions.CultureInvariant, Timeout);
    private static readonly Dictionary<string, Regex> LiteralClosings = new[] { "pre", "code", "script", "style" }
        .ToDictionary(name => name, name => new Regex($@"</{name}\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout), StringComparer.OrdinalIgnoreCase);

    internal static string Extract(string markdown, Func<IReadOnlyDictionary<string, string>, string> render, Dictionary<string, string> placeholders)
    {
        if (!Openings.IsMatch(markdown)) return markdown;
        var openings = Openings.Matches(markdown).ToArray();
        var codeNodes = Markdown.Parse(markdown, LiteralPipeline).Descendants()
            .Where(node => node is CodeBlock or CodeInline).ToArray();
        var attributeSpans = new List<(int Start, int End)>();
        var literalSpans = new List<(int Start, int End)>();
        bool InsideHtml(int index) => attributeSpans.Any(span => index > span.Start && index <= span.End)
            || literalSpans.Any(span => index >= span.Start && index <= span.End);

        // Collect real HTML in source order. Markup shown in Markdown examples must
        // not create attribute or literal spans that extend into the rest of the page.
        for (var position = 0; position < markdown.Length;)
        {
            var markup = HtmlMarkup.Match(markdown, position);
            if (!markup.Success) break;
            if (IsEscaped(markdown, markup.Index) || codeNodes.Any(node => markup.Index >= node.Span.Start
                    && markup.Index <= node.Span.End && !InsideHtml(node.Span.Start)))
            {
                position = markup.Index + 1;
                continue;
            }
            position = markup.Index + markup.Length;
            if (markup.Groups["comment"].Success)
            {
                literalSpans.Add((markup.Index, position - 1));
                continue;
            }
            attributeSpans.Add((markup.Index, position - 1));
            if (LiteralClosings.TryGetValue(markup.Groups["name"].Value, out var closingPattern))
            {
                var closing = closingPattern.Match(markdown, position);
                if (closing.Success)
                {
                    position = closing.Index + closing.Length;
                    literalSpans.Add((markup.Index, position - 1));
                }
            }
        }
        var spans = codeNodes.Where(node => !InsideHtml(node.Span.Start))
            .Select(node => (Start: node.Span.Start, End: node.Span.End)).Concat(literalSpans).ToArray();
        var output = new StringBuilder();
        var offset = 0;
        foreach (var opening in openings)
        {
            if (opening.Index < offset || IsEscaped(markdown, opening.Index)
                || spans.Any(span => opening.Index >= span.Start && opening.Index <= span.End)
                || attributeSpans.Any(span => opening.Index > span.Start && opening.Index <= span.End)) continue;
            var tag = Tags.Match(markdown, opening.Index);
            var end = tag.Success ? tag.Index + tag.Length : MalformedTagEnd(markdown, opening.Index);
            var hasClosing = tag.Success && tag.Groups["self"].Success;
            if (tag.Success && !hasClosing)
            {
                var closing = Closing.Match(markdown, end);
                if (closing.Success && !spans.Any(span => closing.Index <= span.End && closing.Index + closing.Length > span.Start))
                {
                    hasClosing = true;
                    end = closing.Index + closing.Length;
                }
            }
            string html;
            try
            {
                if (!tag.Success) throw new MermaidEmbedException("Diagram attributes must have quoted values separated by whitespace.");
                if (!hasClosing) throw new MermaidEmbedException("Mermaid embeds must be self-closing or have an empty closing tag.");
                html = render(ParseAttributes(tag.Groups["attributes"].Value));
            }
            catch (Exception exception) when (exception is MermaidEmbedException or ArgumentException)
            {
                html = $"<div class=\"doc-mermaid-error\" data-search-exclude=\"true\" role=\"note\">Diagram unavailable: {WebUtility.HtmlEncode(exception.Message)}</div>";
            }
            var placeholder = $"<!--DOC_MERMAID_{Guid.NewGuid():N}-->";
            placeholders.Add(placeholder, html);
            output.Append(markdown.AsSpan(offset, opening.Index - offset)).Append(placeholder);
            offset = end;
        }
        return output.Append(markdown.AsSpan(offset)).ToString();
    }

    private static int MalformedTagEnd(string markdown, int start)
    {
        var lineEnd = markdown.IndexOf('\n', start);
        if (lineEnd < 0) lineEnd = markdown.Length;
        var bracket = markdown.IndexOf('>', start, lineEnd - start);
        return bracket < 0 ? lineEnd : bracket + 1;
    }

    private static bool IsEscaped(string markdown, int start)
    {
        var slashes = 0;
        for (var index = start - 1; index >= 0 && markdown[index] == '\\'; index--) slashes++;
        return slashes % 2 != 0;
    }

    private static IReadOnlyDictionary<string, string> ParseAttributes(string text)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var offset = 0;
        while (offset < text.Length)
        {
            if (string.IsNullOrWhiteSpace(text[offset..])) break;
            var match = Attribute.Match(text, offset);
            if (!match.Success) throw new MermaidEmbedException("Diagram attributes must have quoted values separated by whitespace.");
            var name = match.Groups["name"].Value.ToLowerInvariant();
            if (name is not ("ref" or "slug" or "alt" or "caption") || !attributes.TryAdd(name, WebUtility.HtmlDecode(match.Groups["value"].Value)))
                throw new MermaidEmbedException($"Unknown or duplicate diagram attribute '{name}'.");
            offset += match.Length;
        }
        if (attributes.ContainsKey("ref") == attributes.ContainsKey("slug")
            || attributes.Any(attribute => string.IsNullOrWhiteSpace(attribute.Value)))
            throw new MermaidEmbedException("Provide exactly one non-empty ref or slug attribute; optional alt and caption must also be non-empty.");
        return attributes;
    }
}
