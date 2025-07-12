using System.Net.Http.Json;

namespace MuseDashModTools.Core;

internal sealed partial class GitHubMirrorDownloadService : IGitHubMirrorDownloadService
{
    private const string PrimaryRawMirrorUrl = "https://raw.kkgithub.com/";
    private const string PrimaryReleaseMirrorUrl = "https://kkgithub.com/";
    private const string PrimaryRawModLinksUrl = PrimaryRawMirrorUrl + ModLinksBaseUrl;
    private const string PrimaryModJsonUrl = PrimaryRawModLinksUrl + "Mods.json";
    private const string PrimaryLibJsonUrl = PrimaryRawModLinksUrl + "Libs.json";
    private const string PrimaryModsFolderUrl = PrimaryRawModLinksUrl + "Mods/";
    private const string PrimaryLibsFolderUrl = PrimaryRawModLinksUrl + "Libs/";
    private const string PrimaryMelonLoaderUrl = PrimaryReleaseMirrorUrl + MelonLoaderBaseUrl;
    private const string PrimaryUnityDependencyUrl = PrimaryRawMirrorUrl + UnityDependencyBaseUrl;
    private const string PrimaryCpp2ILUrl = PrimaryReleaseMirrorUrl + Cpp2ILBaseUrl;

    public async Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading MelonLoader and Dependencies from GitHubMirror...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);

        try
        {
            await Downloader.DownloadFileTaskAsync(PrimaryMelonLoaderUrl, Config.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(PrimaryUnityDependencyUrl, Config.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(PrimaryCpp2ILUrl, Config.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.ZLogInformation($"MelonLoader and Dependencies downloaded from GitHubMirror successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download MelonLoader from GitHubMirror");
            return false;
        }
    }

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading mod {mod.Name} from GitHubMirror...");

        if (mod.FileName.IsNullOrEmpty())
        {
            Logger.ZLogError($"Mod {mod.Name} does not have file name");
            return false;
        }

        var downloadLink = PrimaryModsFolderUrl + mod.FileName;
        var path = Path.Combine(Config.ModsFolder, mod.FileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download mod {mod.Name} from GitHubMirror");
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading lib {lib.Name} from GitHubMirror...");

        var downloadLink = PrimaryLibsFolderUrl + lib.FileName;
        var path = Path.Combine(Config.UserLibsFolder, lib.FileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download lib {lib.Name} from GitHubMirror");
            return false;
        }
    }

    public async Task DownloadReleaseByTagAsync(string tag, string osString, CancellationToken cancellationToken = default)
    {
        var releaseBaseUrl = ModToolsReleaseDownloadBaseUrl.Replace(GitHubBaseUrl, PrimaryReleaseMirrorUrl);
        var downloadUrl = $"{releaseBaseUrl}{tag}/MuseDashModTools-{osString}.zip";

        try
        {
            await Downloader.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MuseDashModTools.zip"),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version from GitHubMirror");
        }
    }

    public async Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        if (ReadmeCache.TryGetValue(repoId, out var readme))
        {
            Logger.ZLogInformation($"Using cached Readme for {repoId}");
            return readme;
        }

        Logger.ZLogInformation($"Attempting to fetch Readme for {repoId}");
        readme = await FetchReadmeFromBranchesAsync(repoId, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(readme))
        {
            ReadmeCache[repoId] = readme;
            return readme;
        }

        Logger.ZLogInformation($"Branch readme fetch failed");
        return null;
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching mods from GitHubMirror {PrimaryModJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(PrimaryModJsonUrl, Default.Mod, cancellationToken)
            .Catch<Mod?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch mods from GitHubMirror");
                return AsyncEnumerable.Empty<Mod?>();
            });
    }

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs from GitHubMirror {PrimaryLibJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(PrimaryLibJsonUrl, Default.Lib, cancellationToken)
            .Catch<Lib?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch libs from GitHubMirror");
                return AsyncEnumerable.Empty<Lib?>();
            });
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required MultiThreadDownloader Downloader { get; init; }

    [UsedImplicitly]
    public required ILogger<GitHubMirrorDownloadService> Logger { get; init; }

    #endregion Injections
}