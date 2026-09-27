using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Redot_Documentation.Components.Layout;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Renders parsed class documentation as safe HTML.</summary>
public sealed class ClassDocumentationRenderer
{
    /// <summary>Matches grouped language-specific code blocks.</summary>
    private static readonly Regex CodeBlocksRegex = new(
        @"\[codeblocks\](?<body>.*?)\[/codeblocks\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches standalone code blocks.</summary>
    private static readonly Regex CodeBlockRegex = new(
        @"\[codeblock(?:\s+lang=(?<lang>[a-z0-9_+-]+))?\](?<body>.*?)\[/codeblock\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches GDScript and C# code blocks.</summary>
    private static readonly Regex LanguageCodeBlockRegex = new(
        @"\[(?<lang>gdscript|csharp)\](?<body>.*?)\[/\k<lang>\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches inline code and keyboard tags.</summary>
    private static readonly Regex InlineCodeRegex = new(
        @"\[(?<tag>code|kbd)\](?<body>.*?)\[/\k<tag>\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches typed API references.</summary>
    private static readonly Regex TypedReferenceRegex = new(
        @"\[(?<kind>annotation|constant|constructor|enum|member|method|operator|signal|theme_item)\s+(?<target>[^\]]+)\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Matches parameter references.</summary>
    private static readonly Regex ParameterReferenceRegex = new(
        @"\[param\s+(?<name>[^\]]+)\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Matches labeled URL tags.</summary>
    private static readonly Regex UrlWithLabelRegex = new(
        @"\[url=(?<url>[^\]]+)\](?<label>.*?)\[/url\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches unlabeled URL tags.</summary>
    private static readonly Regex UrlRegex = new(
        @"\[url\](?<url>.*?)\[/url\]",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Matches simple class references.</summary>
    private static readonly Regex SimpleClassReferenceRegex = new(
        @"\[(?<class>@?[A-Za-z_][A-Za-z0-9_]*)\]",
        RegexOptions.Compiled);

    private static readonly Regex SignatureValueRegex = new(
        "(?<string>\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*')|" +
        @"(?<number>(?<![\w.])(?:0x[0-9a-fA-F]+|\d+(?:\.\d+)?)(?![\w.]))|" +
        @"(?<boolean>\b(?:true|false)\b)|(?<keyword>\bnull\b)|" +
        @"(?<identifier>\b[A-Za-z_]\w*\b)|(?<operator>[-+*/%=<>!&|]+)|" +
        @"(?<punctuation>[()\[\]{},.:])",
        RegexOptions.Compiled);

    private static readonly HashSet<string> BuiltInTypes = new(StringComparer.Ordinal)
    {
        "bool", "int", "float", "String", "StringName", "Variant", "void"
    };

    /// <summary>Renders a complete class-reference page.</summary>
    /// <param name="entry">The class to render.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="tableOfContents">Receives headings and their existing fragment IDs when supplied.</param>
    /// <returns>The rendered HTML.</returns>
    public string RenderPage(
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        List<DocumentHeading>? tableOfContents = null)
    {
        tableOfContents?.Clear();
        var context = new RenderContext(tableOfContents);
        var html = new StringBuilder();
        html.Append("<article class=\"class-reference\">");
        html.Append("<header class=\"class-reference-header\"><p class=\"class-reference-kicker\">Class reference</p><h1>")
            .Append(Encode(entry.Name))
            .Append("</h1>");

        if (!string.IsNullOrWhiteSpace(entry.Inherits))
        {
            html.Append("<p class=\"class-inheritance\">Inherits ")
                .Append(RenderClassLink(entry.Inherits, snapshot.Version.Slug))
                .Append("</p>");
        }

        AppendStatus(html, "Deprecated", entry.Deprecated, "deprecated", entry, snapshot, context);
        AppendStatus(html, "Experimental", entry.Experimental, "experimental", entry, snapshot, context);
        if (!string.IsNullOrWhiteSpace(entry.BriefDescription))
            html.Append("<div class=\"class-brief\">").Append(RenderMarkup(entry.BriefDescription, entry, snapshot, context)).Append("</div>");
        html.Append("</header>");

        AppendDescriptionSection(html, "Description", "description", entry.Description, entry, snapshot, context);
        AppendMembers(html, entry, snapshot, context);
        AppendCallables(html, "Constructors", "constructors", "constructor", entry.Constructors, entry, snapshot, context);
        AppendCallables(html, "Methods", "methods", "method", entry.Methods, entry, snapshot, context);
        AppendSignals(html, entry, snapshot, context);
        AppendConstants(html, entry, snapshot, context);
        AppendCallables(html, "Operators", "operators", "operator", entry.Operators, entry, snapshot, context);
        AppendCallables(html, "Annotations", "annotations", "annotation", entry.Annotations, entry, snapshot, context);
        AppendThemeItems(html, entry, snapshot, context);
        AppendTutorials(html, entry.Tutorials, snapshot.Version.Slug, context);

        html.Append("<footer class=\"class-reference-source\">Source revision <code>")
            .Append(Encode(ShortCommit(snapshot.CommitSha)))
            .Append("</code></footer></article>");
        return html.ToString();
    }

    /// <summary>Renders class-reference markup as safe HTML.</summary>
    /// <param name="markup">The source markup.</param>
    /// <param name="currentClass">The class containing the markup.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <returns>The rendered HTML.</returns>
    public string RenderMarkup(
        string markup,
        ClassDocumentationEntry currentClass,
        ClassDocumentationSnapshot snapshot)
        => RenderMarkup(markup, currentClass, snapshot, new RenderContext());

    /// <summary>Renders class-reference markup within a shared page context.</summary>
    /// <param name="markup">The source markup.</param>
    /// <param name="currentClass">The class containing the markup.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    /// <returns>The rendered HTML.</returns>
    private string RenderMarkup(
        string markup,
        ClassDocumentationEntry currentClass,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (string.IsNullOrWhiteSpace(markup))
            return string.Empty;

        var placeholders = new Dictionary<string, string>(StringComparer.Ordinal);
        var blockPlaceholders = new HashSet<string>(StringComparer.Ordinal);
        string transformed = markup.Replace("\r\n", "\n", StringComparison.Ordinal);
        transformed = CodeBlocksRegex.Replace(transformed, match => StorePlaceholder(
            placeholders,
            blockPlaceholders,
            RenderTabbedCodeBlocks(match.Groups["body"].Value, context.NextTabGroupIndex()),
            isBlock: true));
        transformed = CodeBlockRegex.Replace(transformed, match => StorePlaceholder(
            placeholders,
            blockPlaceholders,
            RenderCodeBlock(match.Groups["body"].Value, match.Groups["lang"].Value),
            isBlock: true));
        transformed = LanguageCodeBlockRegex.Replace(transformed, match => StorePlaceholder(
            placeholders,
            blockPlaceholders,
            RenderCodeBlock(match.Groups["body"].Value, match.Groups["lang"].Value),
            isBlock: true));
        transformed = InlineCodeRegex.Replace(transformed, match => StorePlaceholder(
            placeholders,
            blockPlaceholders,
            $"<{(match.Groups["tag"].Value.Equals("kbd", StringComparison.OrdinalIgnoreCase) ? "kbd" : "code")}>{Encode(match.Groups["body"].Value)}</{(match.Groups["tag"].Value.Equals("kbd", StringComparison.OrdinalIgnoreCase) ? "kbd" : "code")}>",
            isBlock: false));

        transformed = Encode(transformed);
        transformed = UrlWithLabelRegex.Replace(transformed, match => RenderUrl(
            WebUtility.HtmlDecode(match.Groups["url"].Value),
            match.Groups["label"].Value));
        transformed = UrlRegex.Replace(transformed, match =>
        {
            string url = WebUtility.HtmlDecode(match.Groups["url"].Value);
            return RenderUrl(url, Encode(url));
        });
        transformed = TypedReferenceRegex.Replace(transformed, match => RenderTypedReference(
            match.Groups["kind"].Value,
            WebUtility.HtmlDecode(match.Groups["target"].Value),
            currentClass,
            snapshot));
        transformed = ParameterReferenceRegex.Replace(
            transformed,
            match => $"<code>{Encode(WebUtility.HtmlDecode(match.Groups["name"].Value))}</code>");
        transformed = SimpleClassReferenceRegex.Replace(transformed, match =>
        {
            string className = match.Groups["class"].Value;
            return snapshot.Classes.ContainsKey(className)
                ? RenderClassLink(className, snapshot.Version.Slug)
                : match.Value;
        });

        transformed = ReplaceFormattingTags(transformed);
        transformed = transformed
            .Replace("[br]", "<br>", StringComparison.OrdinalIgnoreCase)
            .Replace("[lb]", "[", StringComparison.OrdinalIgnoreCase)
            .Replace("[rb]", "]", StringComparison.OrdinalIgnoreCase);

        string[] paragraphs = Regex.Split(transformed.Trim(), @"\n\s*\n");
        transformed = string.Join(
            string.Empty,
            paragraphs.Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
                .Select(paragraph =>
                {
                    string trimmed = paragraph.Trim();
                    return blockPlaceholders.Contains(trimmed)
                        ? trimmed
                        : $"<p>{trimmed.Replace("\n", " ", StringComparison.Ordinal)}</p>";
                }));

        foreach ((string placeholder, string html) in placeholders)
            transformed = transformed.Replace(placeholder, html, StringComparison.Ordinal);
        return transformed;
    }

    /// <summary>Appends a nonempty description section.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="title">The section title.</param>
    /// <param name="id">The section identifier.</param>
    /// <param name="description">The section markup.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendDescriptionSection(
        StringBuilder html,
        string title,
        string id,
        string description,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (string.IsNullOrWhiteSpace(description))
            return;
        context.AddHeading(2, id, title);
        html.Append("<section><h2 id=\"").Append(id).Append("\">").Append(title).Append("</h2>")
            .Append(RenderMarkup(description, entry, snapshot, context)).Append("</section>");
    }

    /// <summary>Appends documented class properties.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendMembers(
        StringBuilder html,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (entry.Members.Count == 0)
            return;
        context.AddHeading(2, "properties", "Properties");
        html.Append("<section><h2 id=\"properties\">Properties</h2><div class=\"class-api-list\">");
        foreach (ClassDocumentationMember member in entry.Members)
        {
            string anchor = MemberAnchor("member", member.Name);
            context.AddHeading(3, anchor, member.Name);
            html.Append("<article class=\"class-api-item\"><h3 id=\"")
                .Append(anchor).Append("\"><code>");
            AppendSignatureType(html, member.Type, snapshot);
            html.Append(' ');
            AppendToken(html, "property", member.Name);
            if (!string.IsNullOrWhiteSpace(member.Default))
            {
                html.Append(' ');
                AppendToken(html, "operator", "=");
                html.Append(' ');
                AppendSignatureValue(html, member.Default);
            }
            html.Append("</code></h3>");
            AppendStatus(html, "Deprecated", member.Deprecated, "deprecated", entry, snapshot, context);
            AppendStatus(html, "Experimental", member.Experimental, "experimental", entry, snapshot, context);
            html.Append(RenderMarkup(member.Description, entry, snapshot, context)).Append("</article>");
        }
        html.Append("</div></section>");
    }

    /// <summary>Appends a group of callable members.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="title">The section title.</param>
    /// <param name="sectionId">The section identifier.</param>
    /// <param name="anchorPrefix">The member-anchor prefix.</param>
    /// <param name="callables">The callables to append.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendCallables(
        StringBuilder html,
        string title,
        string sectionId,
        string anchorPrefix,
        IReadOnlyList<ClassDocumentationCallable> callables,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (callables.Count == 0)
            return;
        context.AddHeading(2, sectionId, title);
        html.Append("<section><h2 id=\"").Append(sectionId).Append("\">").Append(title)
            .Append("</h2><div class=\"class-api-list\">");
        var anchorCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (ClassDocumentationCallable callable in callables)
        {
            string baseAnchor = MemberAnchor(anchorPrefix, callable.Name);
            anchorCounts.TryGetValue(baseAnchor, out int anchorCount);
            anchorCounts[baseAnchor] = ++anchorCount;
            string anchor = anchorCount == 1 ? baseAnchor : $"{baseAnchor}-{anchorCount}";
            context.AddHeading(3, anchor, anchorCount == 1 ? callable.Name : $"{callable.Name} (overload {anchorCount})");
            html.Append("<article class=\"class-api-item\"><h3 id=\"").Append(anchor).Append("\"><code>");
            AppendSignatureType(html, callable.ReturnType, snapshot);
            html.Append(' ');
            AppendToken(html, "function", callable.Name);
            AppendToken(html, "punctuation", "(");
            for (int index = 0; index < callable.Parameters.Count; index++)
            {
                if (index > 0)
                {
                    AppendToken(html, "punctuation", ",");
                    html.Append(' ');
                }
                ClassDocumentationParameter parameter = callable.Parameters[index];
                AppendSignatureType(html, parameter.Type, snapshot);
                html.Append(' ');
                AppendToken(html, "variable", parameter.Name);
                if (!string.IsNullOrWhiteSpace(parameter.Default))
                {
                    html.Append(' ');
                    AppendToken(html, "operator", "=");
                    html.Append(' ');
                    AppendSignatureValue(html, parameter.Default);
                }
            }
            AppendToken(html, "punctuation", ")");
            if (!string.IsNullOrWhiteSpace(callable.Qualifiers))
            {
                html.Append(' ');
                AppendToken(html, "keyword", callable.Qualifiers);
            }
            html.Append("</code></h3>");
            AppendStatus(html, "Deprecated", callable.Deprecated, "deprecated", entry, snapshot, context);
            AppendStatus(html, "Experimental", callable.Experimental, "experimental", entry, snapshot, context);
            html.Append(RenderMarkup(callable.Description, entry, snapshot, context)).Append("</article>");
        }
        html.Append("</div></section>");
    }

    /// <summary>Appends documented signals.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendSignals(
        StringBuilder html,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (entry.Signals.Count == 0)
            return;
        context.AddHeading(2, "signals", "Signals");
        html.Append("<section><h2 id=\"signals\">Signals</h2><div class=\"class-api-list\">");
        foreach (ClassDocumentationSignal signal in entry.Signals)
        {
            string anchor = MemberAnchor("signal", signal.Name);
            context.AddHeading(3, anchor, signal.Name);
            html.Append("<article class=\"class-api-item\"><h3 id=\"").Append(anchor)
                .Append("\"><code>");
            AppendToken(html, "function", signal.Name);
            AppendToken(html, "punctuation", "(");
            for (int index = 0; index < signal.Parameters.Count; index++)
            {
                if (index > 0)
                {
                    AppendToken(html, "punctuation", ",");
                    html.Append(' ');
                }
                ClassDocumentationParameter parameter = signal.Parameters[index];
                AppendSignatureType(html, parameter.Type, snapshot);
                html.Append(' ');
                AppendToken(html, "variable", parameter.Name);
            }
            AppendToken(html, "punctuation", ")");
            html.Append("</code></h3>");
            AppendStatus(html, "Deprecated", signal.Deprecated, "deprecated", entry, snapshot, context);
            AppendStatus(html, "Experimental", signal.Experimental, "experimental", entry, snapshot, context);
            html.Append(RenderMarkup(signal.Description, entry, snapshot, context)).Append("</article>");
        }
        html.Append("</div></section>");
    }

    /// <summary>Appends documented constants.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendConstants(
        StringBuilder html,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (entry.Constants.Count == 0)
            return;
        context.AddHeading(2, "constants", "Constants");
        html.Append("<section><h2 id=\"constants\">Constants</h2><div class=\"class-api-list\">");
        foreach (ClassDocumentationConstant constant in entry.Constants)
        {
            string anchor = MemberAnchor("constant", constant.Name);
            context.AddHeading(3, anchor, constant.Name);
            html.Append("<article class=\"class-api-item\"><h3 id=\"").Append(anchor)
                .Append("\"><code>");
            AppendToken(html, "constant", constant.Name);
            html.Append(' ');
            AppendToken(html, "operator", "=");
            html.Append(' ');
            AppendSignatureValue(html, constant.Value);
            html.Append("</code></h3>");
            if (!string.IsNullOrWhiteSpace(constant.Enum))
                html.Append("<p class=\"class-api-meta\">Enum: ").Append(Encode(constant.Enum)).Append("</p>");
            AppendStatus(html, "Deprecated", constant.Deprecated, "deprecated", entry, snapshot, context);
            AppendStatus(html, "Experimental", constant.Experimental, "experimental", entry, snapshot, context);
            html.Append(RenderMarkup(constant.Description, entry, snapshot, context)).Append("</article>");
        }
        html.Append("</div></section>");
    }

    /// <summary>Appends documented theme items.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendThemeItems(
        StringBuilder html,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (entry.ThemeItems.Count == 0)
            return;
        context.AddHeading(2, "theme-items", "Theme properties");
        html.Append("<section><h2 id=\"theme-items\">Theme properties</h2><div class=\"class-api-list\">");
        foreach (ClassDocumentationThemeItem item in entry.ThemeItems)
        {
            string anchor = MemberAnchor("theme-item", item.Name);
            context.AddHeading(3, anchor, item.Name);
            html.Append("<article class=\"class-api-item\"><h3 id=\"").Append(anchor)
                .Append("\"><code>");
            AppendSignatureType(html, item.Type, snapshot);
            html.Append(' ');
            AppendToken(html, "property", item.Name);
            if (!string.IsNullOrWhiteSpace(item.Default))
            {
                html.Append(' ');
                AppendToken(html, "operator", "=");
                html.Append(' ');
                AppendSignatureValue(html, item.Default);
            }
            html.Append("</code></h3>").Append(RenderMarkup(item.Description, entry, snapshot, context)).Append("</article>");
        }
        html.Append("</div></section>");
    }

    /// <summary>Appends valid tutorial links.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="tutorials">The tutorials to append.</param>
    /// <param name="versionSlug">The documentation version slug.</param>
    private static void AppendTutorials(
        StringBuilder html,
        IReadOnlyList<ClassDocumentationTutorial> tutorials,
        string versionSlug,
        RenderContext context)
    {
        if (tutorials.Count == 0)
            return;
        context.AddHeading(2, "tutorials", "Tutorials");
        html.Append("<section><h2 id=\"tutorials\">Tutorials</h2><ul class=\"class-tutorials\">");
        foreach (ClassDocumentationTutorial tutorial in tutorials)
        {
            string? url = NormalizeUrl(tutorial.Url, versionSlug);
            if (url is null)
                continue;
            string label = string.IsNullOrWhiteSpace(tutorial.Title) ? tutorial.Url : tutorial.Title;
            html.Append("<li><a href=\"").Append(Encode(url)).Append("\">").Append(Encode(label)).Append("</a></li>");
        }
        html.Append("</ul></section>");
    }

    /// <summary>Appends a status notice when present.</summary>
    /// <param name="html">The destination builder.</param>
    /// <param name="label">The status label.</param>
    /// <param name="message">The optional status markup.</param>
    /// <param name="cssClass">The status CSS modifier.</param>
    /// <param name="entry">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <param name="context">The current render context.</param>
    private void AppendStatus(
        StringBuilder html,
        string label,
        string? message,
        string cssClass,
        ClassDocumentationEntry entry,
        ClassDocumentationSnapshot snapshot,
        RenderContext context)
    {
        if (message is null)
            return;
        html.Append("<div class=\"class-status class-status-").Append(cssClass).Append("\"><strong>")
            .Append(label).Append(".</strong>");
        if (!string.IsNullOrWhiteSpace(message))
            html.Append(' ').Append(RenderMarkup(message, entry, snapshot, context));
        html.Append("</div>");
    }

    /// <summary>Renders a typed member reference.</summary>
    /// <param name="kind">The reference kind.</param>
    /// <param name="target">The reference target.</param>
    /// <param name="currentClass">The containing class.</param>
    /// <param name="snapshot">The containing snapshot.</param>
    /// <returns>The rendered link.</returns>
    private string RenderTypedReference(
        string kind,
        string target,
        ClassDocumentationEntry currentClass,
        ClassDocumentationSnapshot snapshot)
    {
        string normalizedKind = kind.ToLowerInvariant();
        string className = currentClass.Name;
        string memberName = target.Trim();
        int separator = memberName.IndexOf('.');
        if (separator > 0)
        {
            className = memberName[..separator];
            memberName = memberName[(separator + 1)..];
        }

        string anchorPrefix = normalizedKind switch
        {
            "member" => "member",
            "theme_item" => "theme-item",
            _ => normalizedKind
        };
        string label = normalizedKind == "method" ? $"{target}()" : target;
        string href = $"{ClassPath(snapshot.Version.Slug, className)}#{MemberAnchor(anchorPrefix, memberName)}";
        return $"<a href=\"{Encode(href)}\"><code>{Encode(label)}</code></a>";
    }

    /// <summary>Replaces supported formatting tags with HTML.</summary>
    /// <param name="value">The encoded markup.</param>
    /// <returns>The formatted HTML.</returns>
    private static string ReplaceFormattingTags(string value)
    {
        foreach ((string tag, string htmlTag) in new[]
                 {
                     ("b", "strong"), ("i", "em"), ("u", "u"), ("s", "s"),
                     ("center", "span")
                 })
        {
            string open = htmlTag == "span" ? "<span class=\"class-text-center\">" : $"<{htmlTag}>";
            value = value.Replace($"[{tag}]", open, StringComparison.OrdinalIgnoreCase)
                .Replace($"[/{tag}]", $"</{htmlTag}>", StringComparison.OrdinalIgnoreCase);
        }
        return value;
    }

    /// <summary>Renders an allowed URL with an encoded label.</summary>
    /// <param name="rawUrl">The source URL.</param>
    /// <param name="encodedLabel">The encoded link label.</param>
    /// <returns>The link, or the label when the URL is rejected.</returns>
    private static string RenderUrl(string rawUrl, string encodedLabel)
    {
        string? url = NormalizeUrl(rawUrl, versionSlug: null);
        return url is null
            ? encodedLabel
            : $"<a href=\"{Encode(url)}\">{encodedLabel}</a>";
    }

    /// <summary>Normalizes an allowed documentation or web URL.</summary>
    /// <param name="rawUrl">The source URL.</param>
    /// <param name="versionSlug">The version used for documentation URLs.</param>
    /// <returns>The normalized URL, or <see langword="null"/> when rejected.</returns>
    private static string? NormalizeUrl(string rawUrl, string? versionSlug)
    {
        string url = WebUtility.HtmlDecode(rawUrl).Trim();
        if (url.StartsWith("$DOCS_URL/", StringComparison.Ordinal))
            return versionSlug is null ? null : $"/en/{Uri.EscapeDataString(versionSlug)}/{url[10..]}";
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? absolute)
            && absolute.Scheme is "http" or "https")
            return absolute.ToString();
        if (url.StartsWith("/", StringComparison.Ordinal)
            && !url.StartsWith("//", StringComparison.Ordinal))
            return url;
        return null;
    }

    /// <summary>Renders a code block.</summary>
    /// <param name="body">The code-block body.</param>
    /// <param name="language">The source language.</param>
    /// <returns>The rendered HTML.</returns>
    private static string RenderCodeBlock(string body, string language)
    {
        string normalizedLanguage = language.ToLowerInvariant() switch
        {
            "csharp" => "csharp",
            "text" => "text",
            _ => "gdscript"
        };
        return $"<pre><code class=\"language-{normalizedLanguage}\">{Encode(body.Trim('\r', '\n'))}</code></pre>";
    }

    /// <summary>Renders a uniquely identified tabbed code-block group.</summary>
    /// <param name="body">The grouped code-block body.</param>
    /// <param name="groupIndex">The group index within the rendered output.</param>
    /// <returns>The rendered tab group.</returns>
    private static string RenderTabbedCodeBlocks(string body, int groupIndex)
    {
        MatchCollection matches = LanguageCodeBlockRegex.Matches(body);
        if (matches.Count == 0)
            return RenderCodeBlock(body, "gdscript");

        string bodyId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)))[..12].ToLowerInvariant();
        string id = $"{bodyId}-{groupIndex}";
        var buttons = new StringBuilder();
        var panels = new StringBuilder();
        for (int index = 0; index < matches.Count; index++)
        {
            Match match = matches[index];
            string language = match.Groups["lang"].Value.ToLowerInvariant();
            string label = language == "csharp" ? "C#" : "GDScript";
            string panelId = $"class-doc-tab-{id}-{index}";
            string activeClass = index == 0 ? " active" : string.Empty;
            buttons.Append("<button class=\"doc-tab-button").Append(activeClass)
                .Append("\" type=\"button\" role=\"tab\" id=\"").Append(panelId).Append("-tab")
                .Append("\" aria-controls=\"").Append(panelId)
                .Append("\" tabindex=\"").Append(index == 0 ? "0" : "-1")
                .Append("\" data-tab-target=\"#").Append(panelId)
                .Append("\" aria-selected=\"").Append(index == 0 ? "true" : "false").Append("\">")
                .Append(label).Append("</button>");
            panels.Append("<div id=\"").Append(panelId).Append("\" class=\"doc-tab-panel").Append(activeClass)
                .Append("\" role=\"tabpanel\" aria-labelledby=\"").Append(panelId).Append("-tab\">")
                .Append(RenderCodeBlock(match.Groups["body"].Value, language))
                .Append("</div>");
        }

        return $"<div class=\"doc-tabs\"><div class=\"doc-tab-buttons\" role=\"tablist\">{buttons}</div><div class=\"doc-tab-panels\">{panels}</div></div>";
    }

    /// <summary>Stores rendered HTML behind a markup-safe placeholder.</summary>
    /// <param name="placeholders">The placeholder map.</param>
    /// <param name="blockPlaceholders">The block-level placeholder set.</param>
    /// <param name="html">The rendered HTML.</param>
    /// <param name="isBlock">Whether the HTML is block-level.</param>
    /// <returns>The placeholder token.</returns>
    /// <exception cref="ArgumentException">The generated placeholder already exists.</exception>
    private static string StorePlaceholder(
        IDictionary<string, string> placeholders,
        ISet<string> blockPlaceholders,
        string html,
        bool isBlock)
    {
        string placeholder = $"CLASSDOCPLACEHOLDER{placeholders.Count}TOKEN";
        placeholders.Add(placeholder, html);
        if (isBlock)
        {
            blockPlaceholders.Add(placeholder);
            return $"\n\n{placeholder}\n\n";
        }
        return placeholder;
    }

    /// <summary>Adds a highlighted type while preserving links to documented classes.</summary>
    private static void AppendSignatureType(StringBuilder html, string type, ClassDocumentationSnapshot snapshot)
    {
        if (snapshot.Classes.ContainsKey(type))
        {
            html.Append("<a href=\"").Append(Encode(ClassPath(snapshot.Version.Slug, type))).Append("\">");
            AppendToken(html, "class-name", type);
            html.Append("</a>");
            return;
        }

        AppendToken(html, type == "void" ? "keyword" : BuiltInTypes.Contains(type) ? "builtin" : "class-name", type);
    }

    /// <summary>Adds highlighted literal and expression tokens from an XML signature value.</summary>
    private static void AppendSignatureValue(StringBuilder html, string value)
    {
        int position = 0;
        foreach (Match match in SignatureValueRegex.Matches(value))
        {
            html.Append(Encode(value[position..match.Index]));
            string tokenType = match.Groups["string"].Success ? "string"
                : match.Groups["number"].Success ? "number"
                : match.Groups["boolean"].Success ? "boolean"
                : match.Groups["keyword"].Success ? "keyword"
                : match.Groups["operator"].Success ? "operator"
                : match.Groups["punctuation"].Success ? "punctuation"
                : value[(match.Index + match.Length)..].TrimStart().StartsWith('(') ? "function" : "constant";
            AppendToken(html, tokenType, match.Value);
            position = match.Index + match.Length;
        }
        html.Append(Encode(value[position..]));
    }

    private static void AppendToken(StringBuilder html, string tokenType, string value)
        => html.Append("<span class=\"token ").Append(tokenType).Append("\">")
            .Append(Encode(value)).Append("</span>");

    /// <summary>Renders a class link.</summary>
    /// <param name="className">The class name.</param>
    /// <param name="versionSlug">The documentation version slug.</param>
    /// <returns>The rendered link.</returns>
    private static string RenderClassLink(string className, string versionSlug)
        => $"<a href=\"{Encode(ClassPath(versionSlug, className))}\"><code>{Encode(className)}</code></a>";

    /// <summary>Builds a versioned class path.</summary>
    /// <param name="versionSlug">The documentation version slug.</param>
    /// <param name="className">The class name.</param>
    /// <returns>The escaped site-relative path.</returns>
    public static string ClassPath(string versionSlug, string className)
        => $"/en/{Uri.EscapeDataString(versionSlug)}/Classes/{Uri.EscapeDataString(className)}";

    /// <summary>Builds a stable member anchor.</summary>
    /// <param name="kind">The member kind.</param>
    /// <param name="memberName">The member name.</param>
    /// <returns>The normalized anchor.</returns>
    public static string MemberAnchor(string kind, string memberName)
    {
        string normalized = Regex.Replace(memberName.ToLowerInvariant(), @"[^a-z0-9@]+", "-").Trim('-');
        return $"{kind.Replace('_', '-')}-{(string.IsNullOrWhiteSpace(normalized) ? "item" : normalized)}";
    }

    /// <summary>HTML-encodes text.</summary>
    /// <param name="value">The text to encode.</param>
    /// <returns>The encoded text.</returns>
    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    /// <summary>Shortens a commit identifier for display.</summary>
    /// <param name="commitSha">The full commit identifier.</param>
    /// <returns>The display identifier.</returns>
    private static string ShortCommit(string commitSha)
        => commitSha.Length > 12 ? commitSha[..12] : commitSha;

    /// <summary>Tracks identifiers within one rendered output.</summary>
    private sealed class RenderContext(List<DocumentHeading>? headings = null)
    {
        /// <summary>Stores the next tab-group index.</summary>
        private int _nextTabGroupIndex;

        /// <summary>Gets and advances the next tab-group index.</summary>
        /// <returns>The next unique index.</returns>
        public int NextTabGroupIndex() => _nextTabGroupIndex++;

        public void AddHeading(int level, string id, string title)
            => headings?.Add(new DocumentHeading(level, id, title));
    }
}
