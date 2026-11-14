using Redot_Documentation.Versioning;

namespace Redot_Documentation.Services;

public sealed record DocumentLink(string Title, string Url);
public sealed record DocumentNeighbors(DocumentLink? Previous, DocumentLink? Next)
{
    public static DocumentNeighbors Empty { get; } = new(null, null);
}

/// <summary>Follows the sidebar's sorted articles, visiting each section overview first.</summary>
public static class DocumentNavigation
{
    public static DocumentNeighbors GetNeighbors(VersionProvider provider, ResolvedDocument document)
    {
        var articles = EnumerateArticles(provider.GetSortedRankings()).ToList();
        int current = articles.FindIndex(article => article.Path == document.FullPath);
        if (current < 0)
            return DocumentNeighbors.Empty;

        DocumentLink Link(Article article) => new(article.DisplayName,
            DocumentPathResolver.PublicUrl(Path.GetRelativePath(
                Path.GetDirectoryName(provider.VersionRoot)!, article.Path)));

        return new(current > 0 ? Link(articles[current - 1]) : null,
            current + 1 < articles.Count ? Link(articles[current + 1]) : null);
    }

    private static IEnumerable<Article> EnumerateArticles(IEnumerable<IRanking> rankings)
    {
        foreach (var ranking in rankings)
        {
            if (ranking is Article article)
                yield return article;
            else if (ranking is Section section)
            {
                if (section.IndexArticle is not null)
                    yield return section.IndexArticle;
                foreach (var child in EnumerateArticles(section.GetSortedRankings()))
                    yield return child;
            }
        }
    }
}
