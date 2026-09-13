using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

namespace Redot_Documentation.Search;

public sealed record SearchDocument(string Title, string Heading, string Url, string Page, string Kind, string Body);
public sealed record SearchHit(string Title, string Heading, string Url, string Kind, string Snippet);
public sealed record SearchResponse(bool Available, IReadOnlyList<SearchHit> Hits, bool HasMore = false);
public interface IDocumentationSearch
{
    SearchResponse Search(string version, string query, string kind = "all", int limit = 30);
}

public static class SearchContent
{
    public static IEnumerable<SearchDocument> Extract(string html, string url, string kind)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        foreach (var node in doc.DocumentNode.SelectNodes("//script|//style|//footer|//button")?.ToArray() ?? []) node.Remove();
        string title = Clean(doc.DocumentNode.SelectSingleNode("//h1")?.InnerText ?? url.Split('/').Last());
        string heading = title, target = url;
        var body = new StringBuilder();
        foreach (var node in doc.DocumentNode.Descendants())
        {
            if (node.Name is "h1" or "h2" or "h3")
            {
                if (body.Length > 0) yield return new(title, heading, target, url, kind, Clean(body.ToString()));
                body.Clear();
                heading = Clean(node.InnerText);
                string anchor = node.GetAttributeValue("id", "");
                target = anchor.Length > 0 ? url + "#" + Uri.EscapeDataString(anchor) : url;
            }
            if (node.NodeType == HtmlNodeType.Text) body.Append(node.InnerText).Append(' ');
        }
        if (body.Length > 0) yield return new(title, heading, target, url, kind, Clean(body.ToString()));
    }
    public static string Clean(string text) => Regex.Replace(HtmlEntity.DeEntitize(text), @"\s+", " ").Trim();
    public static string Normalize(string text) => Regex.Replace(Regex.Replace(text, @"([a-z])([A-Z])", "$1 $2"), @"[_]+", " ").ToLowerInvariant();
    public static IEnumerable<string> TermsForIndex(string text) => Regex.Matches(Normalize(text), @"[\p{L}\p{N}]+(?:[+#]+)?").Select(m => m.Value);
    public static string[] Terms(string text) => Regex.Matches(Normalize(text), @"[\p{L}\p{N}]+(?:[+#]+)?").Select(m => m.Value).Take(12).ToArray();
    public static string Snippet(string body, string query)
    {
        int match = Terms(query).Select(t => body.IndexOf(t, StringComparison.OrdinalIgnoreCase)).Where(i => i >= 0).DefaultIfEmpty(0).Min();
        int start = Math.Max(0, match - 65), length = Math.Min(240, body.Length - start);
        return (start > 0 ? "…" : "") + body.Substring(start, length) + (start + length < body.Length ? "…" : "");
    }
}
