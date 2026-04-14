namespace Euterpe.Core;

internal sealed partial class UpdateService : IUpdateService
{
    private static readonly SemVersion CurrentVersion = SemVersion.Parse(AppVersion);

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Get Current version: {CurrentVersion}");
        Logger.ZLogInformation($"Checking for updates ...");

        var prerelease = Config.UpdateChannel is UpdateChannel.Prerelease;
        var target = await GetReleaseCandidateAsync(prerelease, cancellationToken).ConfigureAwait(true);

        return await HandleReleaseAsync(target, cancellationToken).ConfigureAwait(false);
    }

    private async Task<UpdateTarget?> GetReleaseCandidateAsync(bool prerelease, CancellationToken cancellationToken = default)
    {
        var releases = await DistributionClient.GetAppReleaseAsync(!prerelease, prerelease, cancellationToken).ConfigureAwait(false);
        var release = releases.SingleOrDefault(x => x.Slug == PlatformService.RuntimeIdentifier);
        if (release is null)
        {
            return null;
        }

        var entry = release.Versions.Single();
        var version = SemVersion.Parse(entry.Key);

        return new UpdateTarget(version, entry.Value.DownloadUrl);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IEuterpeDistributionClient DistributionClient { get; init; }

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