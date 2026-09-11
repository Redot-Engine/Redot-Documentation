namespace Redot_Documentation.Services;

using System.Text.RegularExpressions;

public static class HeadingAnchor
{
    private static readonly Regex WhitespaceRegex = new(
        @"\s+",
        RegexOptions.Compiled);

    private static readonly Regex InvalidSlugCharacterRegex = new(
        @"[^a-z0-9\-_ ]",
        RegexOptions.Compiled);

    public static string FromTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        var lowercaseTitle = title.ToLowerInvariant();
        var cleaned = InvalidSlugCharacterRegex.Replace(lowercaseTitle, string.Empty);
        var slug = WhitespaceRegex.Replace(cleaned, "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "section" : slug;
    }

    public static string FromReference(string reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var normalizedReference = reference.Replace('_', ' ');
        var decodedReference = Uri.UnescapeDataString(normalizedReference);
        return FromTitle(decodedReference);
    }
}
