using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class RankingConcurrencyTests : IDisposable
{
    private readonly string _docsRoot = Path.Combine(
        Path.GetTempPath(),
        $"redot-ranking-concurrency-tests-{Guid.NewGuid():N}");

    [Fact]
    public void SortRankings_IsSafeWhenCalledConcurrently()
    {
        VersionProvider provider = CreateProvider();

        Parallel.For(0, 10_000, _ =>
        {
            provider.SortRankings();
            AssertExpectedRankings(provider.GetSortedRankings());
        });

        AssertExpectedRankings(provider.GetSortedRankings());
    }

    [Fact]
    public void GetSortedRankings_ReturnsAReadOnlySnapshot()
    {
        VersionProvider provider = CreateProvider();
        IReadOnlyList<IRanking> rankings = provider.GetSortedRankings();

        Assert.False(rankings is IList<IRanking> list && !list.IsReadOnly);
        AssertExpectedRankings(rankings);
    }

    private VersionProvider CreateProvider()
    {
        Directory.CreateDirectory(Path.Combine(_docsRoot, "About"));
        Directory.CreateDirectory(Path.Combine(_docsRoot, "Community"));
        Directory.CreateDirectory(Path.Combine(_docsRoot, "Contributing"));
        Directory.CreateDirectory(Path.Combine(_docsRoot, "26.1"));
        File.WriteAllText(Path.Combine(_docsRoot, "26.1", "pudding.md"), "# Pudding");

        var provider = new VersionProvider(
            new DocumentationVersion
            {
                Slug = "26.1",
                FriendlyName = "Redot 26.1",
                BranchName = "26.1",
                IsLatestStable = true
            },
            _docsRoot);
        provider.ParseSlugs();
        return provider;
    }

    private static void AssertExpectedRankings(IReadOnlyList<IRanking> rankings)
    {
        Assert.Collection(
            rankings,
            ranking => Assert.Equal("About", Assert.IsType<Section>(ranking).Name),
            ranking => Assert.Equal("Community", Assert.IsType<Section>(ranking).Name),
            ranking => Assert.Equal("Contributing", Assert.IsType<Section>(ranking).Name),
            ranking => Assert.Equal("pudding.md", Assert.IsType<Article>(ranking).Name));
    }

    public void Dispose()
    {
        if (Directory.Exists(_docsRoot))
            Directory.Delete(_docsRoot, recursive: true);
    }
}
