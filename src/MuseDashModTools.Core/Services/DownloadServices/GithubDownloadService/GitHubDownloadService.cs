using System.Net.Http.Json;

namespace MuseDashModTools.Core;

internal sealed partial class GitHubDownloadService : IGitHubDownloadService
{
    private const string RawModLinksUrl = GitHubRawContentBaseUrl + ModLinksBaseUrl;
    private const string ModJsonUrl = RawModLinksUrl + "Mods.json";
    private const string LibJsonUrl = RawModLinksUrl + "Libs.json";
    private const string ModsFolderUrl = RawModLinksUrl + "Mods/";
    private const string LibsFolderUrl = RawModLinksUrl + "Libs/";
    private const string MelonLoaderUrl = GitHubBaseUrl + MelonLoaderBaseUrl;
    private const string UnityDependencyUrl = GitHubRawContentBaseUrl + UnityDependencyBaseUrl;
    private const string Cpp2ILUrl = GitHubBaseUrl + Cpp2ILBaseUrl;

    public async Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading MelonLoader and Dependencies from GitHub...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);

        try
        {
            await Downloader.DownloadFileTaskAsync(MelonLoaderUrl, Config.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(UnityDependencyUrl, Config.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(Cpp2ILUrl, Config.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.ZLogInformation($"MelonLoader and Dependencies downloaded from GitHub successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download MelonLoader from GitHub");
            return false;
        }
    }

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading mod {mod.Name} from GitHub...");

        if (mod.FileName.IsNullOrEmpty())
        {
            Logger.ZLogError($"Mod {mod.Name} does not have file name");
            return false;
        }

        var downloadLink = ModsFolderUrl + mod.FileName;
        var path = Path.Combine(Config.ModsFolder, mod.FileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download mod {mod.Name} from GitHub");
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading lib {libName} from GitHub...");

        var libFileName = libName + ".dll";
        var downloadLink = LibsFolderUrl + libFileName;
        var path = Path.Combine(Config.UserLibsFolder, libFileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download lib {libName} from GitHub");
            return false;
        }
    }

    public async Task DownloadReleaseByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"{ReleaseDownloadBaseUrl}{tag}/MuseDashModTools-{PlatformService.OsString}.zip";

        try
        {
            await Downloader.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MuseDashModTools.zip"),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version from GitHub");
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
        Logger.ZLogInformation($"Fetching mods from GitHub {ModJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(ModJsonUrl, SourceGenerationContext.Default.Mod, cancellationToken)
            .Catch<Mod?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch mods from GitHub");
                return AsyncEnumerable.Empty<Mod?>();
            });
    }

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs from GitHub {LibJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(LibJsonUrl, SourceGenerationContext.Default.Lib, cancellationToken)
            .Catch<Lib?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch libs from GitHub");
                return AsyncEnumerable.Empty<Lib?>();
            });
    }

    #region Injections

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public MultiThreadDownloader Downloader { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<GitHubDownloadService> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}