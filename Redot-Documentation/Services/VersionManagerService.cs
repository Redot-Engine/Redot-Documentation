using System.Text.Json;
using System.Text.RegularExpressions;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.Services;

public class VersionManagerService
{
    public IReadOnlyList<DocumentationVersion> Versions => _configuredVersions;
    public DocumentationVersion LatestStableVersion { get; private set; } = null!;
    public DocumentationVersion NextPrereleaseVersion { get; private set; } = null!;

    private const string VersionFileName = "Versions.json";
    private readonly string _docsRootPath;
    private IReadOnlyList<DocumentationVersion> _configuredVersions = [];
    private Dictionary<string, VersionProvider> _versions = new(StringComparer.OrdinalIgnoreCase);

    public VersionManagerService(IWebHostEnvironment webHostEnvironment)
    {
        _docsRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "docs");
    }

    public void LoadContent()
    {
        string versionFilePath = Path.Combine(_docsRootPath, VersionFileName);
        if (!File.Exists(versionFilePath))
            throw new FileNotFoundException("Documentation version configuration was not found.", versionFilePath);

        List<DocumentationVersion> configuredVersions;
        try
        {
            string fileContent = File.ReadAllText(versionFilePath);
            configuredVersions = JsonSerializer.Deserialize<List<DocumentationVersion>>(fileContent) ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Documentation version configuration '{versionFilePath}' is invalid.",
                exception);
        }

        ValidateConfiguration(configuredVersions);

        var versions = new Dictionary<string, VersionProvider>(StringComparer.OrdinalIgnoreCase);
        foreach (DocumentationVersion configuredVersion in configuredVersions)
        {
            var provider = new VersionProvider(configuredVersion, _docsRootPath);
            provider.ParseSlugs();
            versions.Add(configuredVersion.Slug, provider);
        }

        _configuredVersions = configuredVersions.AsReadOnly();
        _versions = versions;
        LatestStableVersion = configuredVersions.Single(version => version.IsLatestStable);
        NextPrereleaseVersion = configuredVersions.Single(version => version.IsNextPrerelease);
    }

    public VersionProvider GetVersionProvider(string slug)
    {
        if (_versions.TryGetValue(slug, out VersionProvider? provider))
            return provider;

        throw new KeyNotFoundException($"Documentation version '{slug}' is not configured.");
    }

    public bool TryGetVersionProvider(string slug, out VersionProvider? provider)
        => _versions.TryGetValue(slug, out provider);

    private void ValidateConfiguration(IReadOnlyCollection<DocumentationVersion> configuredVersions)
    {
        if (configuredVersions.Count == 0)
            throw new InvalidOperationException("At least one documentation version must be configured.");

        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var branchNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (DocumentationVersion version in configuredVersions)
        {
            ValidateRequiredValue(version.Slug, nameof(version.Slug));
            ValidateRequiredValue(version.FriendlyName, nameof(version.FriendlyName));
            ValidateRequiredValue(version.BranchName, nameof(version.BranchName));

            if (!Regex.IsMatch(version.Slug, @"^[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?$"))
            {
                throw new InvalidOperationException(
                    $"Documentation version slug '{version.Slug}' must be a single URL- and path-safe segment.");
            }

            if (!slugs.Add(version.Slug))
                throw new InvalidOperationException($"Documentation version slug '{version.Slug}' is duplicated.");

            if (!branchNames.Add(version.BranchName))
                throw new InvalidOperationException($"Documentation branch name '{version.BranchName}' is duplicated.");

            if (version.IsLatestStable && version.IsNextPrerelease)
            {
                throw new InvalidOperationException(
                    $"Documentation version '{version.Slug}' cannot be both the latest stable and next prerelease.");
            }

            string versionRootPath = Path.Combine(_docsRootPath, version.Slug);
            if (!Directory.Exists(versionRootPath))
            {
                throw new InvalidOperationException(
                    $"Documentation directory '{versionRootPath}' for version '{version.Slug}' does not exist.");
            }
        }

        ValidateRole(configuredVersions, version => version.IsLatestStable, "latest stable");
        ValidateRole(configuredVersions, version => version.IsNextPrerelease, "next prerelease");
    }

    private static void ValidateRequiredValue(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Documentation version property '{propertyName}' is required.");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException($"Documentation version property '{propertyName}' cannot have surrounding whitespace.");
    }

    private static void ValidateRole(
        IEnumerable<DocumentationVersion> configuredVersions,
        Func<DocumentationVersion, bool> predicate,
        string roleName)
    {
        if (configuredVersions.Count(predicate) != 1)
            throw new InvalidOperationException($"Exactly one documentation version must be the {roleName} version.");
    }

}
