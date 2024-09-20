namespace MuseDashModTools.Services;

public sealed partial class GitHubDownloadService
{
    protected async override Task DownloadAssetAsync(GithubRelease release, CancellationToken cancellationToken = default)
    {
        var asset = Array.Find(release.Assets, x => x.Name.Contains(PlatformService.OsString, StringComparison.OrdinalIgnoreCase));
        if (asset is null)
        {
            Logger.Warning("No asset found for current OS: {OS}", PlatformService.OsString);
            return;
        }

        try
        {
            await Downloader.DownloadFileTaskAsync(asset.BrowserDownloadUrl,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, release.Name),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download new version from GitHub");
        }
    }
}