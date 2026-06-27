namespace Redot_Documentation.Versioning;

public class VersionProvider
{
    public string VersionRoot => $"./docs/{VersionName}/";

    public string VersionName { get; set; } = "latest";
    public Section AboutSection { get; set; } = new("About", "./docs/About/", 0);

    public Section CommunitySection { get; set; } = new("Community", "./docs/Community/", 1);

    public Section? VersionedDocsSection { get; set; } = null;

    private List<IRanking> _sortedRankings = new List<IRanking>();

    private Dictionary<string, string> SlugLookupTable = new();

    public static readonly string[] SlugPrefixes = ["doc_", "abt_", "comm_", "class_", "contrib_"];

    public VersionProvider()
    {
        AboutSection.LoadAndParse();
        AboutSection.SortRankings();
        CommunitySection.LoadAndParse();
        CommunitySection.SortRankings();
    }
    public VersionProvider(string versionName) : this()
    {
        VersionName = versionName;
        if (Directory.Exists(VersionRoot))
        {
            VersionedDocsSection = new Section("Versioned Docs", VersionRoot);
            VersionedDocsSection.LoadAndParse();
            VersionedDocsSection.SortRankings();
        }
    }

    public void SortRankings()
    {
        _sortedRankings.Clear();
        _sortedRankings.Add(AboutSection);
        _sortedRankings.Add(CommunitySection);
        _sortedRankings.Sort();
        if (VersionedDocsSection != null)
        {
            VersionedDocsSection.SortRankings();
            _sortedRankings.AddRange(VersionedDocsSection.GetSortedRankings());
        }
    }
    public IRanking[] GetSortedRankings() => _sortedRankings.ToArray();

    public void ParseSlugs()
    {
        SortRankings();
        SlugLookupTable.Clear();
        foreach (IRanking ranking in GetSortedRankings())
        {
            if (ranking is Section subSection)
                ParseSlugs(subSection);
            else
            {
                SlugLookupTable.Add(ranking.Slug, GetReferentialPath(ranking.Path));
            }
        }
    }

    public static string GetReferentialPath(string path)
    {
        if (path.StartsWith("./"))
            path = "/en" + path.Substring(1);
        path = path.Replace("/docs/", "/");
        return path;
    }

    private void ParseSlugs(Section section)
    {
        if (section.IndexArticle != null)
            SlugLookupTable.Add(section.IndexArticle.Slug, GetReferentialPath(section.IndexArticle.Path));
        foreach (IRanking ranking in section.GetSortedRankings())
        {
            if (ranking is Section subSection)
                ParseSlugs(subSection);
            else
                SlugLookupTable.Add(ranking.Slug, GetReferentialPath(ranking.Path));
        }
    }
    public string GetPathFromSlug(string slug) => SlugLookupTable[slug];

}