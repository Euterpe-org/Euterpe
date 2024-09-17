namespace MuseDashModToolsUI.Services;

public sealed partial class GitHubMirrorDownloadService
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
            var downloadUrl = asset.BrowserDownloadUrl.Replace("https://github.com/", PrimaryReleaseMirrorUrl);
            await Downloader.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, release.Name),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download new version from GitHubMirror");
        }
    }
}