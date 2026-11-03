using System.Collections.Frozen;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Mermaider;
using Mermaider.Models;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.Services;

/// <summary>Snapshots and renders bundled diagrams once, before the site starts serving requests.</summary>
public sealed class MermaidDiagramCatalog
{
    private const string SharedScope = "@shared";
    private const long MaxSourceBytes = 512 * 1024;
    private static readonly string[] SharedSections = ["About", "Community", "Contributing"];
    private static readonly RenderOptions Options = new()
    {
        Bg = "#121216",
        Fg = "#f5f5f5",
        Accent = "#ff704b",
        Muted = "#b0b0b8",
        Font = "Arial",
        Transparent = false,
        SanitizeMode = SanitizeMode.Block
    };

    private readonly string root;
    private readonly FrozenDictionary<string, Diagram> paths;
    private readonly FrozenDictionary<string, Diagram[]> slugs;
    private readonly FrozenDictionary<string, byte[]> images;

    public IReadOnlyList<string> Errors { get; }

    public MermaidDiagramCatalog(IWebHostEnvironment environment, VersionManagerService versions,
        ILogger<MermaidDiagramCatalog> logger)
    {
        root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "docs"));
        var entries = new Dictionary<string, Diagram>(StringComparer.OrdinalIgnoreCase);
        var assets = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var errors = new List<string>();
        var settings = "mermaid-cache-1:" + typeof(MermaidRenderer).Assembly.GetName().Version + ":" + JsonSerializer.Serialize(new
        {
            Options.Bg,
            Options.Fg,
            Options.Accent,
            Options.Muted,
            Options.Font,
            Options.Transparent,
            Options.SanitizeMode
        });
        var cache = Path.Combine(environment.ContentRootPath, "App_Data", "mermaid");
        var scopes = versions.Versions.Select(version => (Root: Path.Combine(root, version.Slug), Scope: version.Slug))
            .Concat(SharedSections.Select(section => (Root: Path.Combine(root, section), Scope: SharedScope)));

        foreach (var scope in scopes)
        {
            foreach (var file in EnumerateFiles(scope.Root))
            {
                var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
                var diagram = new Diagram(relative, scope.Scope, SlugFromFilename(file), "", "", null);
                if (!entries.TryAdd(relative, diagram))
                    throw new InvalidOperationException($"Ambiguous diagram paths: '{entries[relative].RelativePath}' and '{relative}'.");
                try
                {
                    if (!IsRegularPath(file) || new FileInfo(file).Length > MaxSourceBytes)
                        throw new InvalidDataException("Diagram source must be a regular file of at most 512 KiB.");
                    var source = File.ReadAllText(file);
                    var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(settings + "\n" + relative + "\n" + source))).ToLowerInvariant();
                    var cachedFile = Path.Combine(cache, key + ".svg");
                    var checksumFile = cachedFile + ".sha256";
                    var cached = ReadCache(cachedFile, checksumFile);
                    var svg = cached ?? Render(source);

                    // A failed cache write must not make otherwise valid documentation unavailable.
                    if (cached is null)
                    {
                        try
                        {
                            Directory.CreateDirectory(cache);
                            if (!IsRegularPath(cache)) throw new IOException("The diagram cache must not contain symbolic links.");
                            var temporary = cachedFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
                            try
                            {
                                File.WriteAllBytes(temporary, svg);
                                File.WriteAllText(temporary + ".sha256", Convert.ToHexString(SHA256.HashData(svg)));
                                File.Move(temporary, cachedFile, overwrite: true);
                                File.Move(temporary + ".sha256", checksumFile, overwrite: true);
                            }
                            finally
                            {
                                if (File.Exists(temporary)) File.Delete(temporary);
                                if (File.Exists(temporary + ".sha256")) File.Delete(temporary + ".sha256");
                            }
                        }
                        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                        {
                            logger.LogWarning(exception, "Could not persist diagram cache for {Diagram}", relative);
                        }
                    }
                    var imageKey = Convert.ToHexString(SHA256.HashData(svg)).ToLowerInvariant();
                    assets.TryAdd(imageKey, svg);
                    entries[relative] = diagram with { Key = imageKey, Title = ReadTitle(svg) };
                }
                catch (Exception exception) when (exception is MermaidParseException or MermaidRenderException or MermaidSvgException or IOException or UnauthorizedAccessException or XmlException or ArgumentException)
                {
                    errors.Add(relative + ": " + exception.Message);
                    entries[relative] = diagram with { Error = "The diagram could not be rendered. Check its source and the server log." };
                    logger.LogError(exception, "Could not render Mermaid diagram {Diagram}", relative);
                }
            }
        }

        paths = entries.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        slugs = entries.Values.GroupBy(entry => SlugKey(entry.Scope, entry.Slug), StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);
        images = assets.ToFrozenDictionary(StringComparer.Ordinal);
        Errors = errors.AsReadOnly();
    }

    internal static IEnumerable<string> EnumerateFiles(string directory)
    {
        if (!Directory.Exists(directory) || !IsRegularPath(directory)) return [];
        return Directory.EnumerateFiles(directory, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            IgnoreInaccessible = false
        }).Where(file => Path.GetExtension(file).Equals(".mmd", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRegularPath(string path)
    {
        for (var current = path; current is not null; current = Path.GetDirectoryName(current))
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
        return true;
    }

    internal static string SlugFromFilename(string path)
        => Regex.Replace(Path.GetFileNameWithoutExtension(path).ToLowerInvariant(), @"[^\p{L}\p{N}]+", "-", RegexOptions.None, TimeSpan.FromSeconds(1)).Trim('-');

    private static string SlugKey(string scope, string slug) => scope + "/" + slug;

    internal Diagram Resolve(ResolvedDocument document, VersionProvider provider, string? reference, string? slug)
    {
        var documentScope = IsShared(document.RelativePath) ? SharedScope : provider.Version.Slug;
        Diagram? diagram;
        if (slug is not null)
        {
            if (!slugs.TryGetValue(SlugKey(documentScope, slug), out var matches))
                throw new MermaidEmbedException($"No diagram has slug '{slug}' in this documentation scope.");
            if (matches.Length != 1)
                throw new MermaidEmbedException($"Diagram slug '{slug}' is ambiguous: {string.Join(", ", matches.Select(match => match.RelativePath))}. Use ref instead.");
            diagram = matches[0];
        }
        else
        {
            var value = reference!;
            if (value.IndexOfAny(['\\', '?', '#', '\0', ':', '%']) >= 0 || value.StartsWith("//", StringComparison.Ordinal)
                || !value.EndsWith(".mmd", StringComparison.OrdinalIgnoreCase))
                throw new MermaidEmbedException("Diagram ref must be a local .mmd file path.");
            var rooted = value.StartsWith('/');
            var explicitlyShared = rooted && IsShared(value.TrimStart('/'));
            var basePath = rooted
                ? (documentScope == SharedScope || explicitlyShared ? root : provider.VersionRoot)
                : Path.GetDirectoryName(document.FullPath)!;
            var fullPath = Path.GetFullPath(Path.Combine(basePath, value.TrimStart('/')));
            var relative = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            diagram = paths.GetValueOrDefault(relative);
            var allowedScope = explicitlyShared ? SharedScope : documentScope;
            if (diagram is null || !string.Equals(diagram.Scope, allowedScope, StringComparison.OrdinalIgnoreCase))
                throw new MermaidEmbedException($"Diagram ref '{value}' was not found in this documentation scope.");
        }
        if (diagram.Error is not null)
            throw new MermaidEmbedException(diagram.RelativePath + ": " + diagram.Error);
        return diagram;
    }

    private static bool IsShared(string relative)
        => SharedSections.Any(section => relative.StartsWith(section + "/", StringComparison.OrdinalIgnoreCase));

    internal string RenderEmbed(ResolvedDocument document, VersionProvider provider, IReadOnlyDictionary<string, string> attributes)
    {
        var diagram = Resolve(document, provider, attributes.GetValueOrDefault("ref"), attributes.GetValueOrDefault("slug"));
        var alt = attributes.GetValueOrDefault("alt") ?? (string.IsNullOrWhiteSpace(diagram.Title) ? diagram.Slug.Replace('-', ' ') : diagram.Title);
        var caption = attributes.TryGetValue("caption", out var text) ? $"<figcaption>{WebUtility.HtmlEncode(text)}</figcaption>" : "";
        return $"<figure class=\"doc-mermaid\"><div class=\"doc-mermaid-scroll\" tabindex=\"0\" role=\"region\" aria-label=\"Diagram\"><img src=\"/_diagrams/{diagram.Key}.svg\" alt=\"{WebUtility.HtmlEncode(alt)}\" loading=\"lazy\" /></div>{caption}</figure>";
    }

    public IResult Image(string key, HttpContext context)
    {
        if (!images.TryGetValue(key, out var svg)) return Results.NotFound();
        context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        context.Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'; img-src data:; style-src 'unsafe-inline'";
        context.Response.Headers.XContentTypeOptions = "nosniff";
        return Results.Bytes(svg, "image/svg+xml", entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue('"' + key + '"'));
    }

    internal string Fingerprint(string version)
        => string.Join('\n', paths.Values.Where(diagram => diagram.Scope == SharedScope || string.Equals(diagram.Scope, version, StringComparison.OrdinalIgnoreCase))
            .OrderBy(diagram => diagram.RelativePath, StringComparer.Ordinal).Select(diagram => diagram.RelativePath + ":" + diagram.Key + ":" + diagram.Error));

    private static byte[] Render(string source) => Encoding.UTF8.GetBytes(MermaidRenderer.RenderSvg(source, Options));

    private static byte[]? ReadCache(string file, string checksumFile)
    {
        try
        {
            if (!File.Exists(file) || !File.Exists(checksumFile) || !IsRegularPath(file) || !IsRegularPath(checksumFile)
                || new FileInfo(file).Length > 32 * 1024 * 1024 || new FileInfo(checksumFile).Length != 64) return null;
            var svg = File.ReadAllBytes(file);
            // Mermaider's standalone sanitizer rejects renderer-owned CSS. Check the exact
            // persisted renderer output for corruption; generation still uses block-mode sanitization.
            if (Convert.ToHexString(SHA256.HashData(svg)) != File.ReadAllText(checksumFile)) return null;
            ReadTitle(svg);
            return svg;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or XmlException)
        {
            return null;
        }
    }

    private static string ReadTitle(byte[] svg)
    {
        using var stream = new MemoryStream(svg);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var document = XDocument.Load(reader);
        if (document.Root?.Name != XName.Get("svg", "http://www.w3.org/2000/svg")) throw new XmlException("Invalid cached SVG.");
        return document.Root.Elements().FirstOrDefault(element => element.Name.LocalName == "title")?.Value ?? "";
    }

    internal sealed record Diagram(string RelativePath, string Scope, string Slug, string Key, string Title, string? Error);
}

internal sealed class MermaidEmbedException(string message) : Exception(message);
