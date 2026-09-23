using System.Collections.ObjectModel;

namespace Redot_Documentation.Versioning;

public class VersionProvider
{
    public DocumentationVersion Version { get; }
    public string VersionRoot { get; }

    public Section AboutSection { get; set; }

    public Section CommunitySection { get; set; }

    public Section ContributingSection { get; set; }

    public Section? VersionedDocsSection { get; set; } = null;

    private IReadOnlyList<IRanking> _sortedRankings = Array.Empty<IRanking>();

    private IReadOnlyDictionary<string, string> _slugLookupTable =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

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
        IRanking[] commonRankings = [AboutSection, CommunitySection, ContributingSection];
        Array.Sort(commonRankings);

        IRanking[] sortedRankings;
        if (VersionedDocsSection != null)
        {
            VersionedDocsSection.SortRankings();
            sortedRankings = [.. commonRankings, .. VersionedDocsSection.GetSortedRankings()];
        }
        else
            sortedRankings = commonRankings;

        Volatile.Write(
            ref _sortedRankings,
            new ReadOnlyCollection<IRanking>(sortedRankings));
    }

    public IReadOnlyList<IRanking> GetSortedRankings()
        => Volatile.Read(ref _sortedRankings);

    public void ParseSlugs()
    {
        SortRankings();
        var slugLookupTable = new Dictionary<string, string>
        {
            ["doc_class_reference"] = $"/en/{Version.Slug}/Classes"
        };
        foreach (IRanking ranking in GetSortedRankings())
        {
            if (ranking is Section subSection)
                ParseSlugs(subSection, slugLookupTable);
            else
            {
                AddSlug(ranking, slugLookupTable);
            }
        }

        Volatile.Write(
            ref _slugLookupTable,
            new ReadOnlyDictionary<string, string>(slugLookupTable));
    }

    public string GetReferentialPath(string path)
    {
        string relativePath = Path.GetRelativePath(_docsRootPath, Path.GetFullPath(path)).Replace('\\', '/');
        if (relativePath == ".." || relativePath.StartsWith("../", StringComparison.Ordinal))
            throw new InvalidOperationException($"Documentation path '{path}' is outside the docs directory.");

        return $"/en/{relativePath}";
    }

    private void ParseSlugs(Section section, IDictionary<string, string> slugLookupTable)
    {
        if (section.IndexArticle != null)
            AddSlug(section.IndexArticle, slugLookupTable);
        foreach (IRanking ranking in section.GetSortedRankings())
        {
            if (ranking is Section subSection)
                ParseSlugs(subSection, slugLookupTable);
            else
                AddSlug(ranking, slugLookupTable);
        }
    }

    private void AddSlug(IRanking ranking, IDictionary<string, string> slugLookupTable)
    {
        string path = Services.DocumentPathResolver.PublicUrl(Path.GetRelativePath(_docsRootPath, ranking.Path));
        if (slugLookupTable.TryGetValue(ranking.Slug, out string? existingPath))
            throw new InvalidOperationException(
                $"Duplicate documentation slug '{ranking.Slug}' in version '{Version.Slug}': '{existingPath}' and '{path}'.");

        slugLookupTable.Add(ranking.Slug, path);
    }

    public string GetPathFromSlug(string slug)
        => Volatile.Read(ref _slugLookupTable)[slug];

}
