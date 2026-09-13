namespace Redot_Documentation.Versioning;

public class RankingConfig
{
    public string? Slug { get; set; }
    public bool IntermingleArticles { get; set; } = false;
    public string SlugPrefix { get; set; } = "doc_";
    public Dictionary<string, int> RankingPriorities { get; set; } = new();
    public HashSet<string> ExcludedItems { get; set; } = new();

    public RankingConfig()
    {
        ExcludedItems.Add("img");
        ExcludedItems.Add("video");
    }
}
