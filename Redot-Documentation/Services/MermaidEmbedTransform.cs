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
    private static readonly Regex Closing = new(@"\G\s*</mermaid\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Attribute = new("""\G\s+(?<name>[a-zA-Z][a-zA-Z0-9_-]*)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""", RegexOptions.CultureInvariant, Timeout);
    private static readonly Regex Literals = new(@"<!--.*?-->|<(pre|code|script|style)\b[^>]*>.*?</\1\s*>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, Timeout);

    internal static string Extract(string markdown, Func<IReadOnlyDictionary<string, string>, string> render, Dictionary<string, string> placeholders)
    {
        if (!Openings.IsMatch(markdown)) return markdown;
        var openings = Openings.Matches(markdown).ToArray();
        var attributeSpans = openings.Select(opening => Tags.Match(markdown, opening.Index))
            .Where(tag => tag.Success).Select(tag => (Start: tag.Index, End: tag.Index + tag.Length - 1)).ToArray();
        var codeSpans = Markdown.Parse(markdown, LiteralPipeline).Descendants()
            .Where(node => node is CodeBlock || node is CodeInline && !attributeSpans.Any(span => node.Span.Start > span.Start && node.Span.Start <= span.End))
            .Select(node => (Start: node.Span.Start, End: node.Span.End))
            .ToArray();
        var spans = codeSpans.ToList();
        for (var position = 0; position < markdown.Length;)
        {
            var literal = Literals.Match(markdown, position);
            if (!literal.Success) break;
            if (codeSpans.Concat(attributeSpans).Any(span => literal.Index >= span.Start && literal.Index <= span.End))
            {
                // An HTML example must not create a literal span across subsequent article content.
                position = literal.Index + 1;
                continue;
            }
            spans.Add((literal.Index, literal.Index + literal.Length - 1));
            position = literal.Index + literal.Length;
        }
        var output = new StringBuilder();
        var offset = 0;
        foreach (var opening in openings)
        {
            if (opening.Index < offset || IsEscaped(markdown, opening.Index)
                || spans.Any(span => opening.Index >= span.Start && opening.Index <= span.End)) continue;
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
