using System.Net.Http.Json;

namespace MuseDashModToolsUI.Services;

public sealed class GitHubMirrorDownloadService : GitHubServiceBase, IGitHubMirrorDownloadService
{
    private const string PrimaryRawMirrorUrl = "https://raw.kkgithub.com/";
    private const string PrimaryReleaseMirrorUrl = "https://kkgithub.com/";
    private const string PrimaryModLinksUrl = PrimaryRawMirrorUrl + ModLinksBaseUrl + "ModLinks.json";
    private const string PrimaryMelonLoaderUrl = PrimaryReleaseMirrorUrl + MelonLoaderBaseUrl;
    private const string PrimaryUnityDependencyUrl = PrimaryRawMirrorUrl + UnityDependencyBaseUrl;
    private const string PrimaryCpp2ILUrl = PrimaryReleaseMirrorUrl + Cpp2ILBaseUrl;

    public async Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHubMirror {Url}...", PrimaryModLinksUrl);

        try
        {
            var mods = await Client.GetFromJsonAsync<Mod[]>(PrimaryModLinksUrl, cancellationToken).ConfigureAwait(false);
            Logger.Information("Mods fetched from GitHubMirror successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHubMirror");
            return null;
        }
    }

    public async Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading MelonLoader and Dependencies from GitHubMirror...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);

        try
        {
            await Downloader.DownloadFileTaskAsync(PrimaryMelonLoaderUrl, Setting.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(PrimaryUnityDependencyUrl, Setting.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(PrimaryCpp2ILUrl, Setting.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.Information("MelonLoader and Dependencies downloaded from GitHubMirror successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download MelonLoader from GitHubMirror");
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public MultiThreadDownloader Downloader { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}