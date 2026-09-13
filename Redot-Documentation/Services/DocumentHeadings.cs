using System.Text.RegularExpressions;
using Redot_Documentation.Components.Layout;

namespace Redot_Documentation.Services;

public static class DocumentHeadings
{
    private static readonly Regex HeadingRegex = new(
        @"<h([1-6])(?<attributes>[^>]*)>(?<content>.*?)</h\1>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex IdRegex = new(
        @"\sid\s*=\s*[\""'](?<id>[^\""']+)[\""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new(
        @"\s+",
        RegexOptions.Compiled);

    public static string Apply(string html, List<DocumentHeading> tableOfContents)
    {
        tableOfContents.Clear();

        var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return HeadingRegex.Replace(html, match =>
        {
            var level = int.Parse(match.Groups[1].Value);
            var attributes = match.Groups["attributes"].Value;
            var content = match.Groups["content"].Value;
            var title = DecodeHeadingTitle(content);

            var existingIdMatch = IdRegex.Match(attributes);
            if (string.IsNullOrWhiteSpace(title))
            {
                if (existingIdMatch.Success)
                {
                    usedIds.Add(existingIdMatch.Groups["id"].Value);
                }
                return match.Value;
            }
            var id = existingIdMatch.Success
                ? existingIdMatch.Groups["id"].Value
                : HeadingAnchor.FromTitle(title);

            id = EnsureUniqueId(id, usedIds);

            if (existingIdMatch.Success && !string.Equals(existingIdMatch.Groups["id"].Value, id, StringComparison.Ordinal))
            {
                attributes = IdRegex.Replace(attributes, $" id=\"{id}\"", 1);
            }
            else if (!existingIdMatch.Success)
            {
                attributes = $" id=\"{id}\"{attributes}";
            }

            if (level <= 3)
                tableOfContents.Add(new DocumentHeading(level, id, title));
            return $"<h{level}{attributes}>{content}</h{level}>";
        });
    }

    private static string DecodeHeadingTitle(string content)
    {
        var withoutTags = HtmlTagRegex.Replace(content, string.Empty);
        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
        return WhitespaceRegex.Replace(decoded, " ").Trim();
    }

    private static string EnsureUniqueId(string id, HashSet<string> usedIds)
    {
        var normalizedId = string.IsNullOrWhiteSpace(id) ? "section" : id;
        if (usedIds.Add(normalizedId))
        {
            return normalizedId;
        }

        var suffix = 2;
        while (!usedIds.Add($"{normalizedId}-{suffix}"))
        {
            suffix++;
        }

        return $"{normalizedId}-{suffix}";
    }

}
