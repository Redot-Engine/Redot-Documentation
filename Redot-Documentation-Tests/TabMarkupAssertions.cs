using System.Text.RegularExpressions;

namespace Redot_Documentation_Tests;

internal static class TabMarkupAssertions
{
    public static void AssertAccessibleTabs(string html, int expectedCount)
    {
        string[] ids = Regex.Matches(html, "\\bid=\"([^\"]+)\"")
            .Select(match => match.Groups[1].Value).ToArray();
        Assert.Equal(ids.Length, ids.Distinct().Count());
        var buttons = Regex.Matches(html, "<button[^>]*role=\"tab\"[^>]*>");
        Assert.Equal(expectedCount, buttons.Count);
        foreach (Match button in buttons)
        {
            string Attribute(string name) => Regex.Match(button.Value, $"\\b{name}=\"([^\"]+)\"").Groups[1].Value;
            string id = Attribute("id");
            string panelId = Attribute("aria-controls");
            Assert.NotEmpty(id);
            Assert.NotEmpty(panelId);
            Assert.Equal("#" + panelId, Attribute("data-tab-target"));
            Assert.Equal(Attribute("aria-selected") == "true" ? "0" : "-1", Attribute("tabindex"));
            Assert.Matches($"<div id=\"{Regex.Escape(panelId)}\"[^>]*role=\"tabpanel\"[^>]*aria-labelledby=\"{Regex.Escape(id)}\"", html);
        }
    }
}
