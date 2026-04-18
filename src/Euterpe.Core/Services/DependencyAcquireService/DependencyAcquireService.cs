using Euterpe.Contracts.Distribution;

namespace Euterpe.Core;

internal sealed partial class DependencyAcquireService : IDependencyAcquireService
{
    private const int MaxRetries = 3;
    private Dependency[]? _cachedDependencies;

    private DependencyTarget[] MelonLoaderTargets =>
    [
        new("MelonLoader", Config.MelonLoaderZipPath),
        new("UnityDependencies", Config.UnityDependencyZipPath),
        new("Cpp2IL", Config.Cpp2ILExecutablePath),
        new("Cpp2IL-Plugin", Config.Cpp2ILPluginPath)
    ];

    public async Task AcquireForMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var dependencies = await GetDependenciesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var (slug, filePath) in MelonLoaderTargets)
        {
            var entry = dependencies.Single(x => x.Slug == slug).Versions.Single().Value;
            var spec = new DependencySpec(slug, entry.DownloadUrl, filePath, entry.SHA256);
            await EnsureValidAsync(spec, onDownloadStarted, progress, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<string?> GetLatestMelonLoaderVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dependencies = await GetDependenciesAsync(cancellationToken).ConfigureAwait(false);
            return dependencies.Single(x => x.Slug is "MelonLoader").Versions.Single().Key;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to fetch latest MelonLoader version");
            return null;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IEuterpeDistributionClient DistributionClient { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<DependencyAcquireService> Logger { get; init; }

    #endregion Injections
}