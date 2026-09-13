using Microsoft.Extensions.Options;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Loads, synchronizes, and publishes class-documentation snapshots.</summary>
public sealed class ClassDocumentationSyncService : IHostedService, IDisposable
{
    /// <summary>Provides configured documentation versions.</summary>
    private readonly VersionManagerService _versionManager;

    /// <summary>Provides source checkouts.</summary>
    private readonly IClassDocumentationSource _source;

    /// <summary>Parses class XML files.</summary>
    private readonly ClassDocumentationParser _parser;

    /// <summary>Stores published snapshots.</summary>
    private readonly ClassDocumentationCatalog _catalog;

    /// <summary>Contains synchronization settings.</summary>
    private readonly ClassDocumentationOptions _options;

    /// <summary>Records synchronization activity.</summary>
    private readonly ILogger<ClassDocumentationSyncService> _logger;

    /// <summary>Serializes synchronization passes.</summary>
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    /// <summary>Cancels the refresh loop during shutdown.</summary>
    private CancellationTokenSource? _stoppingSource;

    /// <summary>Tracks the background refresh loop.</summary>
    private Task? _refreshTask;

    /// <summary>Initializes the synchronization service.</summary>
    /// <param name="versionManager">The version manager.</param>
    /// <param name="source">The checkout source.</param>
    /// <param name="parser">The XML parser.</param>
    /// <param name="catalog">The snapshot catalog.</param>
    /// <param name="options">The synchronization settings.</param>
    /// <param name="logger">The service logger.</param>
    public ClassDocumentationSyncService(
        VersionManagerService versionManager,
        IClassDocumentationSource source,
        ClassDocumentationParser parser,
        ClassDocumentationCatalog catalog,
        IOptions<ClassDocumentationOptions> options,
        ILogger<ClassDocumentationSyncService> logger)
    {
        _versionManager = versionManager;
        _source = source;
        _parser = parser;
        _catalog = catalog;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Loads cached snapshots and starts background synchronization.</summary>
    /// <param name="cancellationToken">Cancels startup.</param>
    /// <returns>A task that completes after cached snapshots are loaded.</returns>
    /// <exception cref="OperationCanceledException">Startup is canceled.</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Class documentation synchronization is disabled.");
            return;
        }

        await LoadCachedSnapshotsAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _stoppingSource = new CancellationTokenSource();
        _refreshTask = RunRefreshLoopAsync(_stoppingSource.Token);
    }

    /// <summary>Stops background synchronization.</summary>
    /// <param name="cancellationToken">Limits the shutdown wait.</param>
    /// <returns>A task that completes when shutdown finishes.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_stoppingSource is null || _refreshTask is null)
            return;

        await _stoppingSource.CancelAsync();
        try
        {
            await _refreshTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    /// <summary>Releases synchronization resources.</summary>
    public void Dispose()
    {
        _stoppingSource?.Dispose();
        _syncLock.Dispose();
    }

    /// <summary>Loads all valid cached snapshots.</summary>
    /// <param name="cancellationToken">Cancels cache loading.</param>
    /// <returns>A completed task after loading finishes.</returns>
    /// <exception cref="OperationCanceledException">Cache loading is canceled.</exception>
    private Task LoadCachedSnapshotsAsync(CancellationToken cancellationToken)
    {
        foreach (DocumentationVersion version in _versionManager.Versions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClassDocumentationCheckout? checkout = null;
            try
            {
                if (!_source.TryGetCurrent(version, out checkout))
                    continue;

                Publish(checkout!);
                _logger.LogInformation(
                    "Loaded cached class documentation for {Version} at {Commit}.",
                    version.Slug,
                    ShortCommit(checkout!.CommitSha));
            }
            catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
            {
                _logger.LogWarning(exception, "Cached class documentation for {Version} is invalid.", version.Slug);
            }
            finally
            {
                checkout?.Dispose();
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>Runs the initial and periodic synchronization passes.</summary>
    /// <param name="cancellationToken">Stops the loop.</param>
    /// <returns>A task representing the loop.</returns>
    private async Task RunRefreshLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SynchronizeSafelyAsync(cancellationToken);
            using var timer = new PeriodicTimer(_options.RefreshInterval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await SynchronizeSafelyAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The class documentation refresh loop stopped unexpectedly.");
        }
    }

    /// <summary>Runs a synchronization pass without leaking failures.</summary>
    /// <param name="cancellationToken">Cancels synchronization.</param>
    /// <returns>A task representing the pass.</returns>
    /// <exception cref="OperationCanceledException">Synchronization is canceled.</exception>
    private async Task SynchronizeSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SynchronizeAllAsync(requireAvailableSnapshot: false, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to complete class documentation synchronization.");
        }
    }

    /// <summary>Synchronizes every configured version.</summary>
    /// <param name="requireAvailableSnapshot">Whether every version must remain available.</param>
    /// <param name="cancellationToken">Cancels synchronization.</param>
    /// <returns>A task representing the pass.</returns>
    /// <exception cref="OperationCanceledException">Synchronization is canceled.</exception>
    /// <exception cref="InvalidOperationException">A required version has no snapshot.</exception>
    private async Task SynchronizeAllAsync(bool requireAvailableSnapshot, CancellationToken cancellationToken)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            var unavailableVersions = new List<string>();
            foreach (DocumentationVersion version in _versionManager.Versions)
            {
                try
                {
                    await SynchronizeVersionAsync(version, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (!_catalog.TryGetSnapshot(version.Slug, out _))
                        unavailableVersions.Add(version.Slug);
                    _logger.LogError(exception, "Unable to synchronize class documentation for {Version}.", version.Slug);
                }
            }

            if (requireAvailableSnapshot && unavailableVersions.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Class documentation is unavailable for: {string.Join(", ", unavailableVersions)}. " +
                    "No valid cached snapshot could be loaded.");
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>Synchronizes one documentation version.</summary>
    /// <param name="version">The version to synchronize.</param>
    /// <param name="cancellationToken">Cancels synchronization.</param>
    /// <returns>A task representing the synchronization.</returns>
    /// <exception cref="OperationCanceledException">Synchronization is canceled.</exception>
    private async Task SynchronizeVersionAsync(
        DocumentationVersion version,
        CancellationToken cancellationToken)
    {
        using ClassDocumentationCheckout checkout = await _source.PrepareAsync(version, cancellationToken);
        if (!checkout.IsPending && _catalog.TryGetSnapshot(version.Slug, out ClassDocumentationSnapshot? existing)
            && string.Equals(existing!.CommitSha, checkout.CommitSha, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Class documentation for {Version} is already current.", version.Slug);
            return;
        }

        ClassDocumentationSnapshot snapshot = CreateSnapshot(checkout);
        checkout.Promote();
        _catalog.Publish(snapshot);
        _logger.LogInformation(
            "Published {Count} class documents for {Version} at {Commit}.",
            snapshot.Classes.Count,
            version.Slug,
            ShortCommit(snapshot.CommitSha));
    }

    /// <summary>Parses and publishes a checkout.</summary>
    /// <param name="checkout">The checkout to publish.</param>
    /// <exception cref="InvalidDataException">The checkout contains invalid class XML.</exception>
    private void Publish(ClassDocumentationCheckout checkout)
        => _catalog.Publish(CreateSnapshot(checkout));

    /// <summary>Creates a snapshot from a checkout.</summary>
    /// <param name="checkout">The source checkout.</param>
    /// <returns>The parsed snapshot.</returns>
    /// <exception cref="InvalidDataException">The checkout contains invalid class XML.</exception>
    private ClassDocumentationSnapshot CreateSnapshot(ClassDocumentationCheckout checkout)
        => new(
            checkout.Version,
            checkout.CommitSha,
            checkout.SynchronizedAt,
            _parser.ParseDirectories(checkout.ClassDocumentationPaths));

    /// <summary>Shortens a commit identifier for logging.</summary>
    /// <param name="commitSha">The full commit identifier.</param>
    /// <returns>The display identifier.</returns>
    private static string ShortCommit(string commitSha)
        => commitSha.Length > 12 ? commitSha[..12] : commitSha;
}
