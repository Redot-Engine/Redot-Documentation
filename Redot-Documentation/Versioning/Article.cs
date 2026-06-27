namespace Redot_Documentation.Versioning;

public class Article : IRanking
{
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; } = 1024;
    public string Path { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string DisplayName => ((IRanking)this).GetDisplayName();

    public Article()
    {

    }

    public Article(string name, string path, string slug, int ranking = 1024)
    {
        Name = name;
        Path = path;
        int extensionLoc = slug.LastIndexOf('.');
        Slug = extensionLoc > 0 ? slug.Substring(0, extensionLoc) : slug;
        Rank = ranking;
    }

    public int CompareTo(IRanking? other)
    {
        if (other == null) return -1;
        if (Rank == other.Rank)
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        return Rank < other.Rank ? -1 : 1;
    }

}