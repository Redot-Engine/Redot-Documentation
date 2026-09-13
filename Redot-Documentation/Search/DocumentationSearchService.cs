using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Redot_Documentation.Services;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.Search;

public sealed class DocumentationSearchService(VersionManagerService versions, ClassDocumentationCatalog catalog,
    IWebHostEnvironment environment, ILogger<DocumentationSearchService> logger) : BackgroundService, IDocumentationSearch
{
    private readonly object _gate = new();
    private readonly Dictionary<string, OpenIndex> _indexes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Channel<string> _updates = Channel.CreateUnbounded<string>();
    private sealed record OpenIndex(FSDirectory Directory, DirectoryReader Reader) : IDisposable
    {
        public void Dispose() { Reader.Dispose(); Directory.Dispose(); }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        catalog.Changed += Queue;
        try
        {
            foreach (var version in versions.Versions) Queue(version.Slug);
            await foreach (string slug in _updates.Reader.ReadAllAsync(stoppingToken))
            {
                try { await BuildAsync(slug, stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "Search indexing failed for {Version}; retaining previous index", slug); }
            }
        }
        finally { catalog.Changed -= Queue; }
    }
    private void Queue(string slug) => _updates.Writer.TryWrite(slug);
    private async Task BuildAsync(string slug, CancellationToken ct)
    {
        var timer = Stopwatch.StartNew();
        var provider = versions.GetVersionProvider(slug);
        string root = Path.Combine(environment.ContentRootPath, "docs");
        var files = new[] { provider.VersionRoot, Path.Combine(root, "About"), Path.Combine(root, "Community"), Path.Combine(root, "Contributing") }
            .Where(System.IO.Directory.Exists).SelectMany(p => System.IO.Directory.EnumerateFiles(p, "*.md", SearchOption.AllDirectories)).Order().ToArray();
        catalog.TryGetSnapshot(slug, out var snapshot);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes("search-schema-2:" + snapshot?.CommitSha));
        foreach (string file in files)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(root, file)));
            hash.AppendData(await File.ReadAllBytesAsync(file, ct));
        }
        string fingerprint = Convert.ToHexString(hash.GetHashAndReset());
        string cache = Path.Combine(environment.ContentRootPath, "App_Data", "search", slug);
        System.IO.Directory.CreateDirectory(cache);
        string path = Path.Combine(cache, fingerprint);
        string manifest = Path.Combine(cache, "current.txt");
        lock (_gate)
        {
            if (!_indexes.ContainsKey(slug) && File.Exists(manifest))
            {
                string previous = File.ReadAllText(manifest).Trim();
                if (previous.Length == 64 && previous.All(Uri.IsHexDigit)) TryOpen(slug, Path.Combine(cache, previous));
            }
        }
        bool validCache;
        using (var check = FSDirectory.Open(path))
        {
            try { using var reader = DirectoryReader.Open(check); validCache = true; }
            catch { validCache = false; }
        }
        if (!validCache)
        {
            string temp = path + ".building-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var directory = FSDirectory.Open(temp))
                using (var analyzer = new WhitespaceAnalyzer(LuceneVersion.LUCENE_48))
                using (var writer = new IndexWriter(directory, new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer) { Similarity = new BM25Similarity() }))
                {
                    var renderer = new DocRendererService(environment);
                    foreach (string file in files)
                    {
                        ct.ThrowIfCancellationRequested();
                        string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
                        string html = DocumentHeadings.Apply(await renderer.RenderToHtmlAsync(relative, provider, ct), []);
                        string url = "/en/" + string.Join('/', relative[..^3].Split('/').Select(Uri.EscapeDataString));
                        foreach (var entry in SearchContent.Extract(html, url, "guides")) Add(writer, entry);
                    }
                    if (snapshot is not null)
                    {
                        var classRenderer = new ClassDocumentationRenderer();
                        foreach (var entry in snapshot.Classes.Values)
                        {
                            ct.ThrowIfCancellationRequested();
                            foreach (var section in SearchContent.Extract(classRenderer.RenderPage(entry, snapshot), ClassDocumentationRenderer.ClassPath(slug, entry.Name), "classes")) Add(writer, section);
                        }
                    }
                    writer.Commit();
                }
                if (System.IO.Directory.Exists(path)) System.IO.Directory.Delete(path, true);
                System.IO.Directory.Move(temp, path);
            }
            finally { if (System.IO.Directory.Exists(temp)) System.IO.Directory.Delete(temp, true); }
        }
        lock (_gate) { if (!TryOpen(slug, path)) throw new IOException("Cannot open completed search index"); }
        await File.WriteAllTextAsync(manifest + ".tmp", fingerprint, ct);
        File.Move(manifest + ".tmp", manifest, true);
        CleanupObsoleteIndexes(cache, fingerprint);
        logger.LogInformation("Search index {Version}: {Documents} sections, {Milliseconds} ms, {Bytes} bytes", slug, _indexes[slug].Reader.NumDocs, timer.ElapsedMilliseconds, System.IO.Directory.GetFiles(path).Sum(f => new FileInfo(f).Length));
    }
    private void CleanupObsoleteIndexes(string cache, string activeFingerprint)
    {
        // The replacement reader and manifest are ready, and TryOpen disposed the old reader.
        // Only generated fingerprint directories belong to this retention policy.
        try
        {
            foreach (string directory in System.IO.Directory.EnumerateDirectories(cache))
            {
                string name = Path.GetFileName(directory);
                if (name.Length != 64 || !name.All(Uri.IsHexDigit)
                    || string.Equals(name, activeFingerprint, StringComparison.OrdinalIgnoreCase)
                    || (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                    continue;

                try { System.IO.Directory.Delete(directory, recursive: true); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    logger.LogWarning(ex, "Could not remove obsolete search index {Directory}; will retry after the next publication", directory);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Could not clean search cache {Cache}; active search index is unchanged", cache);
        }
    }
    private bool TryOpen(string slug, string path)
    {
        FSDirectory? dir = null;
        try
        {
            dir = FSDirectory.Open(path);
            var next = new OpenIndex(dir, DirectoryReader.Open(dir));
            if (_indexes.Remove(slug, out var old)) old.Dispose();
            _indexes[slug] = next;
            return true;
        }
        catch { dir?.Dispose(); return false; }
    }
    internal static void Add(IndexWriter writer, SearchDocument entry)
    {
        var doc = new Document();
        foreach (var (name, value) in new[] { ("title", entry.Title), ("heading", entry.Heading), ("body", entry.Body) })
        {
            doc.Add(new StoredField(name, value));
            doc.Add(new TextField(name + "Terms", string.Join(' ', SearchContent.TermsForIndex(value)), Field.Store.NO));
        }
        foreach (var (name, value) in new[] { ("url", entry.Url), ("page", entry.Page), ("kind", entry.Kind), ("exact", entry.Heading.ToLowerInvariant()) }) doc.Add(new StringField(name, value, Field.Store.YES));
        if (entry.Kind == "classes")
        {
            string symbol = entry.Heading.Split('(', '=')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
            doc.Add(new StringField("exact", symbol.ToLowerInvariant(), Field.Store.NO));
        }
        writer.AddDocument(doc);
    }
    public SearchResponse Search(string version, string query, string kind = "all", int limit = 30)
    {
        if (!versions.TryGetVersionProvider(version, out _)) return new(false, []);
        limit = Math.Clamp(limit, 1, 100);
        query = query.Length > 200 ? query[..200] : query;
        string[] terms = SearchContent.Terms(query);
        lock (_gate)
        {
            if (!_indexes.TryGetValue(version, out var index)) return new(false, []);
            if (terms.Length == 0) return new(true, []);
            var searcher = new IndexSearcher(index.Reader) { Similarity = new BM25Similarity() };
            var main = new BooleanQuery();
            var content = new BooleanQuery();
            foreach (var term in terms)
            {
                var matches = new BooleanQuery();
                foreach (var (field, boost) in new[] { ("titleTerms", 4f), ("headingTerms", 6f), ("bodyTerms", 1f) })
                {
                    matches.Add(new TermQuery(new Term(field, term)) { Boost = boost }, Occur.SHOULD);
                    if (term.Length >= 3) matches.Add(new PrefixQuery(new Term(field, term)) { Boost = boost * .4f }, Occur.SHOULD);
                }
                content.Add(matches, Occur.MUST);
            }
            main.Add(content, Occur.MUST);
            main.Add(new TermQuery(new Term("exact", query.Trim().ToLowerInvariant())) { Boost = 15 }, Occur.SHOULD);
            if (kind is "guides" or "classes") main.Add(new TermQuery(new Term("kind", kind)), Occur.MUST);
            var hits = CollectDistinctPages(searcher, main, limit + 1);
            if (hits.Count == 0 && terms.All(t => t.Length >= 4))
            {
                var fuzzy = new BooleanQuery();
                foreach (string term in terms) fuzzy.Add(new FuzzyQuery(new Term("bodyTerms", term), 1), Occur.MUST);
                if (kind is "guides" or "classes") fuzzy.Add(new TermQuery(new Term("kind", kind)), Occur.MUST);
                hits = CollectDistinctPages(searcher, fuzzy, limit + 1);
            }
            return new(true, hits.Take(limit).Select(d => new SearchHit(d.Get("title"), d.Get("heading"), d.Get("url"), d.Get("kind"), SearchContent.Snippet(d.Get("body"), query))).ToArray(), hits.Count > limit);
        }
    }
    internal static IReadOnlyList<Document> CollectDistinctPages(IndexSearcher searcher, Query query, int count)
    {
        const int batchSize = 128;
        var pages = new HashSet<string>(StringComparer.Ordinal);
        var hits = new List<Document>();
        ScoreDoc? after = null;
        while (hits.Count < count)
        {
            var batch = searcher.SearchAfter(after, query, batchSize);
            foreach (var score in batch.ScoreDocs)
            {
                var document = searcher.Doc(score.Doc);
                if (pages.Add(document.Get("page"))) hits.Add(document);
                if (hits.Count == count) return hits;
            }
            if (batch.ScoreDocs.Length < batchSize) break;
            after = batch.ScoreDocs[^1];
        }
        return hits;
    }
    public override void Dispose()
    {
        base.Dispose();
        lock (_gate) { foreach (var index in _indexes.Values) index.Dispose(); _indexes.Clear(); }
    }
}
