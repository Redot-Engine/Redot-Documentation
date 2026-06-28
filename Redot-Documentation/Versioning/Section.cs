using System.Text.Json;

namespace Redot_Documentation.Versioning;

using FilePath = System.IO.Path;

public sealed class Section : IRanking
{
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; } = 1024;
    public string Path { get; set; } = string.Empty;

    public Article? IndexArticle { get; set; }

    public List<Article> Articles { get; set; } = new List<Article>();

    public List<Section> SubSections { get; set; } = new List<Section>();

    public bool IntermingleArticles { get; set; } = false;

    public string Slug { get; set; } = string.Empty;

    public string SlugPrefix { get; set; } = "doc_";

    private List<IRanking> _sortedRankings = new List<IRanking>();

    public string DisplayName => ((IRanking)this).GetDisplayName();

    public Section()
    {

    }

    public Section(string name, string path, int rank = 1024)
    {
        Name = name;
        Path = path;
        Rank = rank;
    }
    public void LoadAndParse()
    {
        DirectoryInfo directory = new DirectoryInfo(Path);
        if (!directory.Exists) return;
        FileInfo sectionConfig = new FileInfo(FilePath.Combine(directory.FullName, "index.json"));
        Dictionary<string, int> rankingPriorities;
        HashSet<string> excludedItems;
        if (sectionConfig.Exists)
        {
            string config = File.ReadAllText(sectionConfig.FullName);
            var configData = JsonSerializer.Deserialize<RankingConfig>(config);
            if (configData is null) configData = new RankingConfig();
            rankingPriorities = configData.RankingPriorities;
            excludedItems = configData.ExcludedItems;
            SlugPrefix = configData.SlugPrefix;
            IntermingleArticles = configData.IntermingleArticles;
        }
        else
        {
            rankingPriorities = new Dictionary<string, int>();
            excludedItems = new HashSet<string>();
            excludedItems.Add("img");
            excludedItems.Add("video");
        }
        if (string.IsNullOrEmpty(Slug))
            Slug = Name.ToLowerInvariant();
        FileInfo[] files = directory.GetFiles("*.md");
        foreach (FileInfo file in files)
        {
            if (excludedItems.Contains(file.Name)) continue;
            int priority = rankingPriorities.GetValueOrDefault(file.Name, 1024);
            Article article = new Article(file.Name, FilePath.Combine(Path, file.Name), SlugPrefix + file.Name, priority);
            if (article.Name == "index" || article.Name == "index.md")
            {
                IndexArticle = article;
                IndexArticle.Name = Name;
                IndexArticle.Slug = SlugPrefix + Slug;
            }
            else
                Articles.Add(article);
        }

        DirectoryInfo[] subDirectories = directory.GetDirectories();
        foreach (DirectoryInfo subDirectory in subDirectories)
        {
            if (excludedItems.Contains(subDirectory.Name)) continue;
            int priority = rankingPriorities.GetValueOrDefault(subDirectory.Name, 1024);
            Section subSection = new Section(subDirectory.Name, FilePath.Combine(Path, subDirectory.Name), priority);
            SubSections.Add(subSection);
            subSection.LoadAndParse();
            subSection.SortRankings();
        }
    }
    public int CompareTo(IRanking? other)
    {
        if (other == null) return -1;
        if (Rank == other.Rank)
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        return Rank < other.Rank ? -1 : 1;
    }

    public void SortRankings()
    {
        Articles.Sort();
        SubSections.Sort();
        _sortedRankings.Clear();
        _sortedRankings.AddRange(Articles);
        _sortedRankings.AddRange(SubSections);
        if (IntermingleArticles)
            _sortedRankings.Sort();
    }

    public IRanking[] GetSortedRankings() => _sortedRankings.ToArray();

}