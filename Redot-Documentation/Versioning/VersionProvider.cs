namespace Redot_Documentation.Versioning;

public class VersionProvider
{
    public DocumentationVersion Version { get; }
    public string VersionRoot { get; }

    public Section AboutSection { get; set; }

    public Section CommunitySection { get; set; }

    public Section ContributingSection { get; set; }

    public Section? VersionedDocsSection { get; set; } = null;

    private List<IRanking> _sortedRankings = new List<IRanking>();

    private Dictionary<string, string> SlugLookupTable = new();

    private readonly string _docsRootPath;

    public static readonly string[] SlugPrefixes = ["doc_", "abt_", "comm_", "class_", "contrib_"];

    public VersionProvider(DocumentationVersion version, string docsRootPath)
    {
        Version = version;
        _docsRootPath = Path.GetFullPath(docsRootPath);
        VersionRoot = Path.Combine(_docsRootPath, version.Slug);

        AboutSection = new Section("About", Path.Combine(_docsRootPath, "About"), 0);
        CommunitySection = new Section("Community", Path.Combine(_docsRootPath, "Community"), 1);
        ContributingSection = new Section("Contributing", Path.Combine(_docsRootPath, "Contributing"), 2);

        AboutSection.LoadAndParse();
        AboutSection.SortRankings();
        CommunitySection.LoadAndParse();
        CommunitySection.SortRankings();
        ContributingSection.LoadAndParse();
        ContributingSection.SortRankings();
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
        _sortedRankings.Add(ContributingSection);
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

    public string GetReferentialPath(string path)
    {
        string relativePath = Path.GetRelativePath(_docsRootPath, Path.GetFullPath(path)).Replace('\\', '/');
        if (relativePath == ".." || relativePath.StartsWith("../", StringComparison.Ordinal))
            throw new InvalidOperationException($"Documentation path '{path}' is outside the docs directory.");

        return $"/en/{relativePath}";
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
