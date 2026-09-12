using System.Text.Json;
using Microsoft.Extensions.Options;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Provides version-specific class-documentation checkouts.</summary>
public interface IClassDocumentationSource
{
    /// <summary>Prepares a current or pending checkout.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="cancellationToken">Cancels checkout preparation.</param>
    /// <returns>The prepared checkout.</returns>
    /// <exception cref="OperationCanceledException">Preparation is canceled.</exception>
    /// <exception cref="InvalidOperationException">The source configuration or checkout is invalid.</exception>
    Task<ClassDocumentationCheckout> PrepareAsync(
        DocumentationVersion version,
        CancellationToken cancellationToken);

    /// <summary>Gets the active cached checkout when available.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="checkout">The active checkout, when found.</param>
    /// <returns><see langword="true"/> when a compatible checkout exists; otherwise, <see langword="false"/>.</returns>
    bool TryGetCurrent(
        DocumentationVersion version,
        out ClassDocumentationCheckout? checkout);
}

/// <summary>Maintains sparse Git checkouts of class documentation.</summary>
public sealed class GitClassDocumentationSource : IClassDocumentationSource
{
    /// <summary>Defines the active repository directory name.</summary>
    private const string RepositoryDirectoryName = "repository";

    /// <summary>Defines the synchronization metadata file name.</summary>
    private const string MetadataFileName = "sync.json";

    /// <summary>Stores metadata inside the checkout so it is promoted with the repository.</summary>
    private const string RepositoryMetadataFileName = ".redot-class-doc-sync.json";

    /// <summary>Runs Git commands.</summary>
    private readonly IGitCommandRunner _git;

    /// <summary>Contains source settings.</summary>
    private readonly ClassDocumentationOptions _options;

    /// <summary>Contains the absolute cache root path.</summary>
    private readonly string _cacheRoot;

    /// <summary>Records source activity.</summary>
    private readonly ILogger<GitClassDocumentationSource> _logger;

    /// <summary>Initializes the Git checkout source.</summary>
    /// <param name="git">The Git command runner.</param>
    /// <param name="options">The source settings.</param>
    /// <param name="environment">The web-host environment.</param>
    /// <param name="logger">The source logger.</param>
    public GitClassDocumentationSource(
        IGitCommandRunner git,
        IOptions<ClassDocumentationOptions> options,
        IWebHostEnvironment environment,
        ILogger<GitClassDocumentationSource> logger)
    {
        _git = git;
        _options = options.Value;
        _logger = logger;
        _cacheRoot = Path.GetFullPath(Path.IsPathRooted(_options.CacheRoot)
            ? _options.CacheRoot
            : Path.Combine(environment.ContentRootPath, _options.CacheRoot));
    }

    /// <inheritdoc />
    public async Task<ClassDocumentationCheckout> PrepareAsync(
        DocumentationVersion version,
        CancellationToken cancellationToken)
    {
        ValidateOptions();
        Directory.CreateDirectory(_cacheRoot);
        RecoverInterruptedPromotion(version);

        ClassDocumentationSyncMetadata? currentMetadata = ReadMetadata(version);
        string? currentCommit = IsMetadataCompatible(currentMetadata, version)
            ? currentMetadata!.CommitSha
            : null;
        string remoteCommit = await GetRemoteCommitAsync(version, cancellationToken);
        string activeRepositoryPath = GetActiveRepositoryPath(version);
        string activeClassDocumentationPath = GetClassDocumentationPath(activeRepositoryPath);
        if (string.Equals(currentCommit, remoteCommit, StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(Path.Combine(activeRepositoryPath, ".git"))
            && Directory.Exists(activeClassDocumentationPath))
        {
            return ClassDocumentationCheckout.Current(
                version,
                remoteCommit,
                activeRepositoryPath,
                activeClassDocumentationPath);
        }

        string versionRoot = GetVersionRoot(version);
        string stagingRoot = Path.Combine(versionRoot, $"staging-{Guid.NewGuid():N}");
        string stagingRepositoryPath = Path.Combine(stagingRoot, RepositoryDirectoryName);
        Directory.CreateDirectory(stagingRoot);

        try
        {
            await _git.RunAsync(
                [
                    "clone",
                    "--depth", "1",
                    "--filter=blob:none",
                    "--sparse",
                    "--single-branch",
                    "--no-tags",
                    "--branch", version.BranchName,
                    "--", _options.RepositoryUrl,
                    stagingRepositoryPath
                ],
                versionRoot,
                _options.GitTimeout,
                cancellationToken);

            await _git.RunAsync(
                ["-C", stagingRepositoryPath, "sparse-checkout", "set", "--cone", "--", _options.RepositoryPath],
                versionRoot,
                _options.GitTimeout,
                cancellationToken);

            GitCommandResult commitResult = await _git.RunAsync(
                ["-C", stagingRepositoryPath, "rev-parse", "HEAD"],
                versionRoot,
                _options.GitTimeout,
                cancellationToken);

            WriteMetadataFile(
                Path.Combine(stagingRepositoryPath, RepositoryMetadataFileName),
                new ClassDocumentationSyncMetadata(
                    commitResult.StandardOutput,
                    version.BranchName,
                    _options.RepositoryUrl,
                    DateTimeOffset.UtcNow));

            return ClassDocumentationCheckout.Pending(
                version,
                commitResult.StandardOutput,
                stagingRepositoryPath,
                GetClassDocumentationPath(stagingRepositoryPath),
                stagingRoot,
                () => Promote(version, stagingRepositoryPath, stagingRoot));
        }
        catch
        {
            DeleteDirectoryIfPresent(stagingRoot);
            throw;
        }
    }

    /// <inheritdoc />
    public bool TryGetCurrent(
        DocumentationVersion version,
        out ClassDocumentationCheckout? checkout)
    {
        RecoverInterruptedPromotion(version);
        ClassDocumentationSyncMetadata? metadata = ReadMetadata(version);
        string repositoryPath = GetActiveRepositoryPath(version);
        string classDocumentationPath = GetClassDocumentationPath(repositoryPath);
        if (!IsMetadataCompatible(metadata, version) || !Directory.Exists(classDocumentationPath))
        {
            checkout = null;
            return false;
        }

        checkout = ClassDocumentationCheckout.Current(
            version,
            metadata!.CommitSha,
            repositoryPath,
            classDocumentationPath,
            metadata.SynchronizedAt);
        return true;
    }

    /// <summary>Gets the remote commit for a version branch.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="cancellationToken">Cancels the Git query.</param>
    /// <returns>The remote commit identifier.</returns>
    /// <exception cref="OperationCanceledException">The query is canceled.</exception>
    /// <exception cref="InvalidOperationException">The branch is absent or Git fails.</exception>
    private async Task<string> GetRemoteCommitAsync(
        DocumentationVersion version,
        CancellationToken cancellationToken)
    {
        GitCommandResult result = await _git.RunAsync(
            ["ls-remote", "--heads", "--", _options.RepositoryUrl, $"refs/heads/{version.BranchName}"],
            _cacheRoot,
            _options.GitTimeout,
            cancellationToken);

        string? commit = result.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 2 && string.Equals(
                parts[1],
                $"refs/heads/{version.BranchName}",
                StringComparison.Ordinal))
            .Select(parts => parts[0])
            .SingleOrDefault();

        return !string.IsNullOrWhiteSpace(commit)
            ? commit
            : throw new InvalidOperationException(
                $"The class documentation branch '{version.BranchName}' was not found in '{_options.RepositoryUrl}'.");
    }

    /// <summary>Atomically promotes a staging checkout.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="stagingRepositoryPath">The staging repository path.</param>
    /// <param name="stagingRoot">The staging root path.</param>
    /// <exception cref="IOException">A cache move or metadata write fails.</exception>
    /// <exception cref="UnauthorizedAccessException">A cache path cannot be modified.</exception>
    private void Promote(
        DocumentationVersion version,
        string stagingRepositoryPath,
        string stagingRoot)
    {
        string versionRoot = GetVersionRoot(version);
        string activeRepositoryPath = GetActiveRepositoryPath(version);
        string backupRepositoryPath = Path.Combine(versionRoot, "repository.previous");
        ClassDocumentationSyncMetadata metadata = ReadMetadataFile(
            Path.Combine(stagingRepositoryPath, RepositoryMetadataFileName), version)
            ?? throw new InvalidDataException("The staged checkout has no valid synchronization metadata.");

        DeleteDirectoryIfPresent(backupRepositoryPath);
        if (Directory.Exists(activeRepositoryPath))
            Directory.Move(activeRepositoryPath, backupRepositoryPath);

        try
        {
            Directory.Move(stagingRepositoryPath, activeRepositoryPath);
            try
            {
                WriteMetadata(version, metadata);
            }
            catch
            {
                DeleteDirectoryIfPresent(activeRepositoryPath);
                if (Directory.Exists(backupRepositoryPath))
                    Directory.Move(backupRepositoryPath, activeRepositoryPath);
                throw;
            }

            TryDeleteAfterPromotion(backupRepositoryPath);
            TryDeleteAfterPromotion(stagingRoot);
        }
        catch
        {
            if (!Directory.Exists(activeRepositoryPath) && Directory.Exists(backupRepositoryPath))
                Directory.Move(backupRepositoryPath, activeRepositoryPath);
            throw;
        }
    }

    /// <summary>Attempts noncritical post-promotion cleanup.</summary>
    /// <param name="path">The directory to delete.</param>
    private void TryDeleteAfterPromotion(string path)
    {
        try
        {
            DeleteDirectoryIfPresent(path);
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Unable to clean up class documentation cache path {Path}.", path);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "Unable to clean up class documentation cache path {Path}.", path);
        }
    }

    /// <summary>Restores a backup or finishes metadata publication after an interrupted promotion.</summary>
    /// <param name="version">The documentation version.</param>
    /// <exception cref="IOException">The backup cannot be restored.</exception>
    /// <exception cref="UnauthorizedAccessException">The cache path cannot be modified.</exception>
    private void RecoverInterruptedPromotion(DocumentationVersion version)
    {
        string versionRoot = GetVersionRoot(version);
        string activeRepositoryPath = GetActiveRepositoryPath(version);
        string backupRepositoryPath = Path.Combine(versionRoot, "repository.previous");
        if (!Directory.Exists(activeRepositoryPath) && Directory.Exists(backupRepositoryPath))
        {
            _logger.LogWarning("Recovering the previous class documentation checkout for {Version}.", version.Slug);
            Directory.Move(backupRepositoryPath, activeRepositoryPath);
        }

        // This file moves atomically with the checkout. It remains authoritative if
        // the process stops after Directory.Move but before replacing sync.json.
        string repositoryMetadataPath = Path.Combine(activeRepositoryPath, RepositoryMetadataFileName);
        if (File.Exists(repositoryMetadataPath))
        {
            ClassDocumentationSyncMetadata? metadata = ReadMetadataFile(repositoryMetadataPath, version);
            if (metadata is not null
                && metadata != ReadMetadataFile(Path.Combine(versionRoot, MetadataFileName), version))
                WriteMetadata(version, metadata);
        }
    }

    /// <summary>Writes synchronization metadata atomically.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="metadata">The metadata to write.</param>
    /// <exception cref="IOException">The metadata cannot be written.</exception>
    /// <exception cref="UnauthorizedAccessException">The metadata path cannot be modified.</exception>
    private void WriteMetadata(DocumentationVersion version, ClassDocumentationSyncMetadata metadata)
    {
        string versionRoot = GetVersionRoot(version);
        Directory.CreateDirectory(versionRoot);
        string metadataPath = Path.Combine(versionRoot, MetadataFileName);
        WriteMetadataFile(metadataPath, metadata);
    }

    /// <summary>Durably stages metadata before atomically replacing its destination.</summary>
    private static void WriteMetadataFile(string metadataPath, ClassDocumentationSyncMetadata metadata)
    {
        string temporaryPath = $"{metadataPath}.{Guid.NewGuid():N}.tmp";
        using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, metadata);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporaryPath, metadataPath, overwrite: true);
    }

    /// <summary>Reads compatible synchronization metadata.</summary>
    /// <param name="version">The documentation version.</param>
    /// <returns>The metadata, or <see langword="null"/> when absent or invalid.</returns>
    private ClassDocumentationSyncMetadata? ReadMetadata(DocumentationVersion version)
    {
        string repositoryMetadataPath = Path.Combine(GetActiveRepositoryPath(version), RepositoryMetadataFileName);
        // Fall back only for legacy checkouts, never for an invalid new metadata file.
        return ReadMetadataFile(File.Exists(repositoryMetadataPath)
            ? repositoryMetadataPath
            : Path.Combine(GetVersionRoot(version), MetadataFileName), version);
    }

    /// <summary>Reads metadata without substituting metadata from another checkout.</summary>
    private ClassDocumentationSyncMetadata? ReadMetadataFile(string metadataPath, DocumentationVersion version)
    {
        if (!File.Exists(metadataPath))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ClassDocumentationSyncMetadata>(File.ReadAllText(metadataPath));
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            _logger.LogWarning(exception, "Ignoring invalid class documentation metadata for {Version}.", version.Slug);
            return null;
        }
    }

    /// <summary>Checks metadata against the current source settings.</summary>
    /// <param name="metadata">The metadata to check.</param>
    /// <param name="version">The documentation version.</param>
    /// <returns><see langword="true"/> when the metadata is compatible; otherwise, <see langword="false"/>.</returns>
    private bool IsMetadataCompatible(
        ClassDocumentationSyncMetadata? metadata,
        DocumentationVersion version)
        => metadata is not null
            && string.Equals(metadata.BranchName, version.BranchName, StringComparison.Ordinal)
            && string.Equals(metadata.RepositoryUrl, _options.RepositoryUrl, StringComparison.Ordinal);

    /// <summary>Gets and creates a version cache root.</summary>
    /// <param name="version">The documentation version.</param>
    /// <returns>The absolute version root.</returns>
    /// <exception cref="InvalidOperationException">The version path escapes the cache root.</exception>
    private string GetVersionRoot(DocumentationVersion version)
    {
        string path = Path.GetFullPath(Path.Combine(_cacheRoot, version.Slug));
        string cacheRootWithSeparator = _cacheRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(cacheRootWithSeparator, StringComparison.Ordinal))
            throw new InvalidOperationException($"Version slug '{version.Slug}' resolves outside the class documentation cache.");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Gets the active repository path for a version.</summary>
    /// <param name="version">The documentation version.</param>
    /// <returns>The active repository path.</returns>
    /// <exception cref="InvalidOperationException">The version path escapes the cache root.</exception>
    private string GetActiveRepositoryPath(DocumentationVersion version)
        => Path.Combine(GetVersionRoot(version), RepositoryDirectoryName);

    /// <summary>Gets the class-documentation path within a checkout.</summary>
    /// <param name="repositoryPath">The repository root.</param>
    /// <returns>The absolute class-documentation path.</returns>
    /// <exception cref="InvalidOperationException">The configured path escapes the checkout.</exception>
    private string GetClassDocumentationPath(string repositoryPath)
    {
        string path = Path.GetFullPath(Path.Combine(repositoryPath, _options.RepositoryPath));
        string repositoryPathWithSeparator = Path.GetFullPath(repositoryPath).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(repositoryPathWithSeparator, StringComparison.Ordinal))
            throw new InvalidOperationException("ClassDocumentation:RepositoryPath resolves outside the checkout.");
        return path;
    }

    /// <summary>Validates source settings.</summary>
    /// <exception cref="InvalidOperationException">A required setting is invalid.</exception>
    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.RepositoryUrl))
            throw new InvalidOperationException("ClassDocumentation:RepositoryUrl is required.");
        if (string.IsNullOrWhiteSpace(_options.RepositoryPath) || Path.IsPathRooted(_options.RepositoryPath))
            throw new InvalidOperationException("ClassDocumentation:RepositoryPath must be a relative repository path.");
        if (_options.RefreshInterval <= TimeSpan.Zero)
            throw new InvalidOperationException("ClassDocumentation:RefreshInterval must be greater than zero.");
        if (_options.GitTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException("ClassDocumentation:GitTimeout must be greater than zero.");
    }

    /// <summary>Deletes a directory when it exists.</summary>
    /// <param name="path">The directory path.</param>
    /// <exception cref="IOException">The directory cannot be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">The directory cannot be accessed.</exception>
    private static void DeleteDirectoryIfPresent(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    /// <summary>Describes a promoted checkout.</summary>
    /// <param name="CommitSha">The source commit identifier.</param>
    /// <param name="BranchName">The source branch name.</param>
    /// <param name="RepositoryUrl">The source repository URL.</param>
    /// <param name="SynchronizedAt">The promotion time.</param>
    private sealed record ClassDocumentationSyncMetadata(
        string CommitSha,
        string BranchName,
        string RepositoryUrl,
        DateTimeOffset SynchronizedAt);
}

/// <summary>Represents a current or pending class-documentation checkout.</summary>
public sealed class ClassDocumentationCheckout : IDisposable
{
    /// <summary>Promotes a pending checkout.</summary>
    private readonly Action? _promote;

    /// <summary>Contains the disposable staging root.</summary>
    private readonly string? _stagingRoot;

    /// <summary>Tracks whether a pending checkout was promoted.</summary>
    private bool _promoted;

    /// <summary>Initializes a checkout.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="commitSha">The source commit identifier.</param>
    /// <param name="repositoryPath">The repository path.</param>
    /// <param name="classDocumentationPath">The class-documentation path.</param>
    /// <param name="isPending">Whether the checkout awaits promotion.</param>
    /// <param name="synchronizedAt">The synchronization time.</param>
    /// <param name="stagingRoot">The optional staging root.</param>
    /// <param name="promote">The optional promotion action.</param>
    private ClassDocumentationCheckout(
        DocumentationVersion version,
        string commitSha,
        string repositoryPath,
        string classDocumentationPath,
        bool isPending,
        DateTimeOffset synchronizedAt,
        string? stagingRoot,
        Action? promote)
    {
        Version = version;
        CommitSha = commitSha;
        RepositoryPath = repositoryPath;
        ClassDocumentationPath = classDocumentationPath;
        IsPending = isPending;
        SynchronizedAt = synchronizedAt;
        _stagingRoot = stagingRoot;
        _promote = promote;
    }

    /// <summary>Gets the documentation version.</summary>
    public DocumentationVersion Version { get; }

    /// <summary>Gets the source commit identifier.</summary>
    public string CommitSha { get; }

    /// <summary>Gets the repository path.</summary>
    public string RepositoryPath { get; }

    /// <summary>Gets the class-documentation path.</summary>
    public string ClassDocumentationPath { get; }

    /// <summary>Gets whether the checkout awaits promotion.</summary>
    public bool IsPending { get; }

    /// <summary>Gets the synchronization time.</summary>
    public DateTimeOffset SynchronizedAt { get; }

    /// <summary>Promotes this checkout once.</summary>
    /// <exception cref="IOException">The checkout cannot be promoted.</exception>
    /// <exception cref="UnauthorizedAccessException">The cache path cannot be modified.</exception>
    public void Promote()
    {
        if (!IsPending || _promoted)
            return;
        _promote!();
        _promoted = true;
    }

    /// <summary>Deletes an unpromoted staging checkout.</summary>
    /// <exception cref="IOException">The staging checkout cannot be deleted.</exception>
    /// <exception cref="UnauthorizedAccessException">The staging path cannot be accessed.</exception>
    public void Dispose()
    {
        if (IsPending && !_promoted && _stagingRoot is not null && Directory.Exists(_stagingRoot))
            Directory.Delete(_stagingRoot, recursive: true);
    }

    /// <summary>Creates a pending checkout.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="commitSha">The source commit identifier.</param>
    /// <param name="repositoryPath">The staging repository path.</param>
    /// <param name="classDocumentationPath">The class-documentation path.</param>
    /// <param name="stagingRoot">The staging root.</param>
    /// <param name="promote">The promotion action.</param>
    /// <returns>The pending checkout.</returns>
    internal static ClassDocumentationCheckout Pending(
        DocumentationVersion version,
        string commitSha,
        string repositoryPath,
        string classDocumentationPath,
        string stagingRoot,
        Action promote)
        => new(
            version,
            commitSha,
            repositoryPath,
            classDocumentationPath,
            isPending: true,
            DateTimeOffset.UtcNow,
            stagingRoot,
            promote);

    /// <summary>Creates an active checkout.</summary>
    /// <param name="version">The documentation version.</param>
    /// <param name="commitSha">The source commit identifier.</param>
    /// <param name="repositoryPath">The active repository path.</param>
    /// <param name="classDocumentationPath">The class-documentation path.</param>
    /// <param name="synchronizedAt">The optional synchronization time.</param>
    /// <returns>The active checkout.</returns>
    internal static ClassDocumentationCheckout Current(
        DocumentationVersion version,
        string commitSha,
        string repositoryPath,
        string classDocumentationPath,
        DateTimeOffset? synchronizedAt = null)
        => new(
            version,
            commitSha,
            repositoryPath,
            classDocumentationPath,
            isPending: false,
            synchronizedAt ?? DateTimeOffset.UtcNow,
            null,
            null);
}
