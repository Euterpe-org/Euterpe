namespace Euterpe.Core;

internal sealed partial class UpdateService : IUpdateService
{
    public SemVersion CurrentVersion { get; init; } = SemVersion.Parse(AppVersion);

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
        var release = releases.SingleOrDefault(x => x.Slug == PlatformInfo.RuntimeIdentifier);
        if (release is null)
        {
            return null;
        }

        var entry = release.Versions.Single();
        var version = SemVersion.Parse(entry.Key);

        return new UpdateTarget(version, entry.Value.DownloadUrl);
    }

    #region Injections

    public required Config Config { get; init; }
    public required IEuterpeDistributionClient DistributionClient { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required ILogger<UpdateService> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IPlatformInfo PlatformInfo { get; init; }

    #endregion Injections
}
