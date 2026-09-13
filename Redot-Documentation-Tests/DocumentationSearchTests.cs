using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Redot_Documentation.Search;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;
using Redot_Documentation.ClassDocumentation;

namespace Redot_Documentation_Tests;

public sealed class DocumentationSearchTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CollectDistinctPages_SearchesBeyondFirstThousandSections(int requestedPages)
    {
        using var directory = new Lucene.Net.Store.RAMDirectory();
        using var analyzer = new Lucene.Net.Analysis.Core.WhitespaceAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
        using (var writer = new Lucene.Net.Index.IndexWriter(directory,
            new Lucene.Net.Index.IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer)))
        {
            for (int i = 0; i < 1100; i++)
                DocumentationSearchService.Add(writer, new("Dense", "Section", $"/dense#{i}", "/dense", "guides", "needle"));
            DocumentationSearchService.Add(writer, new("Second", "Section", "/second", "/second", "guides", "needle"));
            DocumentationSearchService.Add(writer, new("Third", "Section", "/third", "/third", "guides", "needle"));
            writer.Commit();
        }
        using var reader = Lucene.Net.Index.DirectoryReader.Open(directory);
        var searcher = new Lucene.Net.Search.IndexSearcher(reader);
        var query = new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("bodyTerms", "needle"));
        var hits = DocumentationSearchService.CollectDistinctPages(searcher, query, requestedPages);
        Assert.Equal(new[] { "/dense", "/second", "/third" }.Take(requestedPages), hits.Select(d => d.Get("page")));
        Assert.Equal("/dense#0", hits[0].Get("url"));
    }

    [Fact]
    public async Task Index_SearchesBodyAndMembers_IsolatesVersions_AndRefreshesSnapshots()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            foreach (string version in new[] { "stable", "latest" })
            {
                Directory.CreateDirectory(Path.Combine(root, "docs", version));
                await File.WriteAllTextAsync(Path.Combine(root, "docs", version, "player.md"),
                    "# Coding the player\n\n## Physics\n\n" + (version == "stable" ? "Detect collisions with walls." : "Future physics changes."));
            }
            await File.WriteAllTextAsync(Path.Combine(root, "docs", "Versions.json"), JsonSerializer.Serialize(new[] {
                new DocumentationVersion { Slug="stable",FriendlyName="Stable",BranchName="stable",IsLatestStable=true },
                new DocumentationVersion { Slug="latest",FriendlyName="Latest",BranchName="master",IsNextPrerelease=true }
            }));
            var env = new TestEnvironment(root);
            var versions = new VersionManagerService(env); versions.LoadContent();
            var catalog = new ClassDocumentationCatalog();
            var logger = new TestLogger();
            using var service = new DocumentationSearchService(versions, catalog, env, logger);
            await service.StartAsync(default);
            await Wait(() => service.Search("latest", "physics").Available);
            string cache = Path.Combine(root, "App_Data", "search", "stable");
            string manifest = Path.Combine(cache, "current.txt");
            await Wait(() => File.Exists(manifest));
            string initialIndex = Path.Combine(cache, File.ReadAllText(manifest));
            string temporary = Path.Combine(cache, new string('A', 64) + ".building-test");
            string unrelated = Path.Combine(cache, "notes");
            Directory.CreateDirectory(temporary);
            Directory.CreateDirectory(unrelated);
            var hits = service.Search("stable", "detect collisions").Hits;
            Assert.Single(hits);
            Assert.EndsWith("player#physics", hits[0].Url);
            Assert.Contains("collisions", hits[0].Snippet);
            Assert.Empty(service.Search("latest", "collisions").Hits);
            Assert.Empty(service.Search("stable", "collisions", "classes").Hits);
            Assert.Single(service.Search("stable", "colisions").Hits);
            catalog.Publish(new(versions.LatestStableVersion, "revision1", DateTimeOffset.UtcNow,
                new Dictionary<string, ClassDocumentationEntry> { ["CharacterBody3D"] = new() { Name = "CharacterBody3D", Description = "Sliding collision motion", Methods = [new() { Name = "move_and_slide", Description = "Move while sliding along walls." }] } }));
            await Wait(() => service.Search("stable", "character body", "classes").Hits.Count == 1);
            await Wait(() => !Directory.Exists(initialIndex));
            string firstClassIndex = Path.Combine(cache, File.ReadAllText(manifest));
            Assert.True(Directory.Exists(firstClassIndex));
            Assert.EndsWith("#method-move-and-slide", service.Search("stable", "move_and_slide", "classes").Hits[0].Url);
            catalog.Publish(new(versions.LatestStableVersion, "revision2", DateTimeOffset.UtcNow,
                new Dictionary<string, ClassDocumentationEntry> { ["NewClass"] = new() { Name = "NewClass", Description = "Replacement snapshot" } }));
            await Wait(() => service.Search("stable", "replacement", "classes").Hits.Count == 1);
            await Wait(() => !Directory.Exists(firstClassIndex));
            string activeIndex = Path.Combine(cache, File.ReadAllText(manifest));
            Assert.True(Directory.Exists(activeIndex));
            Assert.Single(Directory.GetDirectories(cache), p => Path.GetFileName(p).Length == 64);
            Assert.True(Directory.Exists(temporary));
            Assert.True(Directory.Exists(unrelated));
            Assert.Single(service.Search("latest", "physics").Hits);
            Assert.Empty(service.Search("stable", "character body", "classes").Hits);
            env.ContentRootPath = Path.Combine(root, "missing-root");
            catalog.Publish(new(versions.LatestStableVersion, "revision3", DateTimeOffset.UtcNow,
                new Dictionary<string, ClassDocumentationEntry>()));
            await Wait(() => logger.Errors > 0);
            Assert.True(Directory.Exists(activeIndex));
            Assert.Equal(activeIndex, Path.Combine(cache, File.ReadAllText(manifest)));
            Assert.Single(service.Search("stable", "replacement", "classes").Hits);
            env.ContentRootPath = root;
            catalog.Publish(new(versions.LatestStableVersion, "revision2", DateTimeOffset.UtcNow,
                new Dictionary<string, ClassDocumentationEntry> { ["NewClass"] = new() { Name = "NewClass", Description = "Replacement snapshot" } }));
            await service.StopAsync(default);
            using var cached = new DocumentationSearchService(versions, catalog, env, NullLogger<DocumentationSearchService>.Instance);
            await cached.StartAsync(default);
            await Wait(() => cached.Search("stable", "replacement").Hits.Count == 1);
            await cached.StopAsync(default);
        }
        finally { Directory.Delete(root, true); }
    }
    private static async Task Wait(Func<bool> ready)
    {
        for (int i = 0; i < 200; i++) { if (ready()) return; await Task.Delay(50); }
        Assert.True(ready(), "Search index did not become ready");
    }
    private sealed class TestLogger : ILogger<DocumentationSearchService>
    {
        public int Errors;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { if (level == LogLevel.Error) Interlocked.Increment(ref Errors); }
    }
    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = root;
        public string WebRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
