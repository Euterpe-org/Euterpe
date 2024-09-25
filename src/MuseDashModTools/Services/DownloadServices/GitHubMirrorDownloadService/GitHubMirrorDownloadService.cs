using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MuseDashModTools.Services;

public sealed partial class GitHubMirrorDownloadService : GitHubServiceBase, IGitHubMirrorDownloadService
{
    private const string PrimaryRawMirrorUrl = "https://raw.kkgithub.com/";
    private const string PrimaryReleaseMirrorUrl = "https://kkgithub.com/";
    private const string PrimaryRawModLinksUrl = PrimaryRawMirrorUrl + ModLinksBaseUrl;
    private const string PrimaryModLinksUrl = PrimaryRawModLinksUrl + "ModLinks.json";
    private const string PrimaryModsFolderUrl = PrimaryRawModLinksUrl + "Mods/";
    private const string PrimaryLibsFolderUrl = PrimaryRawModLinksUrl + "Libs/";
    private const string PrimaryMelonLoaderUrl = PrimaryReleaseMirrorUrl + MelonLoaderBaseUrl;
    private const string PrimaryUnityDependencyUrl = PrimaryRawMirrorUrl + UnityDependencyBaseUrl;
    private const string PrimaryCpp2ILUrl = PrimaryReleaseMirrorUrl + Cpp2ILBaseUrl;

    public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = SemVersion.Parse(AppVersion);
        Logger.Information("Checking for updates from GitHub... Current version: {Version}", currentVersion);

        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppName, AppVersion));
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", Setting.GitHubToken);

        if (!Setting.DownloadPrerelease)
        {
            var latestRelease = await GetLatestReleaseAsync(cancellationToken).ConfigureAwait(true);
            await HandleReleaseAsync(latestRelease, cancellationToken).ConfigureAwait(true);
        }
        else
        {
            var prerelease = await GetPrereleaseAsync(cancellationToken).ConfigureAwait(true);
            await HandleReleaseAsync(prerelease, cancellationToken).ConfigureAwait(true);
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

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading mod {ModName} from GitHubMirror...", mod.Name);

        if (mod.DownloadLink.IsNullOrEmpty())
        {
            Logger.Error("Mod {ModName} download link is empty", mod.Name);
            return false;
        }

        var downloadLink = PrimaryModsFolderUrl + mod.DownloadLink;
        var path = Path.Combine(Setting.ModsFolder, mod.IsLocal ? mod.FileNameWithoutExtension + mod.FileExtension : mod.DownloadLink);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download mod {ModName} from GitHubMirror", mod.Name);
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading lib {LibName} from GitHubMirror...", libName);

        var libFileName = libName + ".dll";
        var downloadLink = PrimaryLibsFolderUrl + libFileName;
        var path = Path.Combine(Setting.UserLibsFolder, libFileName);
        try
        {
            var stream = await Client.GetStreamAsync(downloadLink, cancellationToken).ConfigureAwait(false);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download lib {LibName} from GitHubMirror", libName);
            return false;
        }
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHubMirror {Url}...", PrimaryModLinksUrl);

        try
        {
            var mods = Client.GetFromJsonAsAsyncEnumerable<Mod>(PrimaryModLinksUrl, cancellationToken);
            Logger.Information("Mods fetched from GitHubMirror successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHubMirror");
            return AsyncEnumerable.Empty<Mod?>();
        }
    }

    #region Injections

    [UsedImplicitly]
    public override HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public MultiThreadDownloader Downloader { get; init; } = null!;

    [UsedImplicitly]
    public override ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public override Setting Setting { get; init; } = null!;

    #endregion Injections
}