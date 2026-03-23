namespace Euterpe.Core;

internal sealed partial class UpdateService : IUpdateService
{
    private static readonly SemVersion _currentVersion = SemVersion.Parse(AppVersion);

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Get Current version: {_currentVersion}");
        Logger.ZLogInformation($"Checking for updates from RSS...");

        var release = Config.UpdateChannel switch
        {
            UpdateChannel.Stable => await GetStableReleaseFromRSSAsync(cancellationToken).ConfigureAwait(true),
            UpdateChannel.Prerelease => await GetPrereleaseFromRSSAsync(cancellationToken).ConfigureAwait(true),
            _ => throw new UnreachableException()
        };

        return await HandleRSSReleaseAsync(release, cancellationToken).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<UpdateService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}