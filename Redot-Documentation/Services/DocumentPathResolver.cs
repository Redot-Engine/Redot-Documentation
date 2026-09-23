using System.Collections.Frozen;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.Services;

/// <summary>Maps URL aliases to the exact spelling of bundled Markdown files.</summary>
public sealed class DocumentPathResolver
{
    private readonly FrozenDictionary<string, ResolvedDocument> documents;

    private readonly VersionManagerService versions;

    public DocumentPathResolver(IWebHostEnvironment environment, VersionManagerService versions)
    {
        this.versions = versions;
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "docs"));
        var entries = new Dictionary<string, ResolvedDocument>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in EnumerateMarkdownFiles(root))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            var key = NormalizePath(relative);
            var document = new ResolvedDocument(file, relative, PublicUrl(relative));
            if (!entries.TryAdd(key, document))
                throw new InvalidOperationException(
                    $"Ambiguous documentation paths: '{entries[key].RelativePath}' and '{relative}'.");
        }
        documents = entries.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    internal static IEnumerable<string> EnumerateMarkdownFiles(string root)
    {
        if (!Directory.Exists(root) || (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
            return [];
        // Share the same file boundary with search indexing, including its fingerprint reads.
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            IgnoreInaccessible = false
        };
        return Directory.EnumerateFiles(root, "*", options)
            .Where(file => Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase));
    }

    public ResolvedDocument? Resolve(string path, VersionProvider provider)
    {
        var key = NormalizePath(path);
        var firstSegment = key.Split('/', 2)[0];
        if (!versions.TryGetVersionProvider(firstSegment, out _)
            && documents.TryGetValue(key, out var shared))
            return shared;
        return documents.GetValueOrDefault($"{provider.Version.Slug}/{key}");
    }

    public ResolvedDocument? ResolveRoute(string path, VersionManagerService versions)
    {
        var (provider, documentPath) = ParseRoute(path, versions);
        return Resolve(documentPath, provider);
    }

    public static (VersionProvider Provider, string Path) ParseRoute(string path, VersionManagerService versions)
    {
        var normalized = path.Trim('/');
        var separator = normalized.IndexOf('/');
        var first = separator < 0 ? normalized : normalized[..separator];
        if (versions.TryGetVersionProvider(first, out var provider))
            return (provider!, separator < 0 ? "" : normalized[(separator + 1)..]);
        return (versions.GetVersionProvider(versions.LatestStableVersion.Slug), normalized);
    }

    // Input is a decoded route path, never a full URL. Do not decode a second time.
    public static string NormalizePath(string path)
    {
        if (path.Contains('\\') || path.Contains('?') || path.Contains('#') || path.Contains('\0'))
            throw new ArgumentException("Invalid documentation path.", nameof(path));
        var normalized = path.Trim('/');
        if (normalized.Split('/').Any(segment => segment is "." or ".."))
            throw new ArgumentException("Invalid documentation path.", nameof(path));
        if (normalized.Length == 0)
            return "index";
        if (normalized.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            return normalized[..^5];
        if (normalized.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            return normalized[..^3];
        return normalized;
    }

    public static string PublicUrl(string markdownRelativePath)
    {
        var path = markdownRelativePath.Replace('\\', '/');
        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            path = path[..^3];
        return "/en/" + string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
    }
}

public sealed record ResolvedDocument(string FullPath, string RelativePath, string PublicUrl);
