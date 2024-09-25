using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MuseDashModTools.Services;

public sealed partial class GitHubDownloadService : GitHubServiceBase, IGitHubDownloadService
{
    private const string RawGitHubUrl = "https://raw.githubusercontent.com/";
    private const string ReleaseGitHubUrl = "https://github.com/";
    private const string RawModLinksUrl = RawGitHubUrl + ModLinksBaseUrl;
    private const string ModLinksUrl = RawModLinksUrl + "ModLinks.json";
    private const string ModsFolderUrl = RawModLinksUrl + "Mods/";
    private const string LibsFolderUrl = RawModLinksUrl + "Libs/";
    private const string MelonLoaderUrl = ReleaseGitHubUrl + MelonLoaderBaseUrl;
    private const string UnityDependencyUrl = RawGitHubUrl + UnityDependencyBaseUrl;
    private const string Cpp2ILUrl = ReleaseGitHubUrl + Cpp2ILBaseUrl;

    public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = SemVersion.Parse(AppVersion);
        Logger.Information("Checking for updates from GitHub... Current version: {Version}", currentVersion);

        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppName, AppVersion));
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!Setting.GitHubToken.IsNullOrEmpty())
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", Setting.GitHubToken);
        }

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
        Logger.Information("Downloading MelonLoader and Dependencies from GitHub...");

        Downloader.DownloadStarted += onDownloadStarted;
        Downloader.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);

        try
        {
            await Downloader.DownloadFileTaskAsync(MelonLoaderUrl, Setting.MelonLoaderZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(UnityDependencyUrl, Setting.UnityDependencyZipPath, cancellationToken).ConfigureAwait(false);
            await Downloader.DownloadFileTaskAsync(Cpp2ILUrl, Setting.Cpp2ILZipPath, cancellationToken).ConfigureAwait(false);
            Logger.Information("MelonLoader and Dependencies downloaded from GitHub successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to download MelonLoader from GitHub");
            return false;
        }
    }

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading mod {ModName} from GitHub...", mod.Name);

        if (mod.DownloadLink.IsNullOrEmpty())
        {
            Logger.Error("Mod {ModName} download link is empty", mod.Name);
            return false;
        }

        var downloadLink = ModsFolderUrl + mod.DownloadLink;
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
            Logger.Error(ex, "Failed to download mod {ModName} from GitHub", mod.Name);
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default)
    {
        Logger.Information("Downloading lib {LibName} from GitHub...", libName);

        var libFileName = libName + ".dll";
        var downloadLink = LibsFolderUrl + libFileName;
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
            Logger.Error(ex, "Failed to download lib {LibName} from GitHub", libName);
            return false;
        }
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHub {Url}...", ModLinksUrl);

        try
        {
            var mods = Client.GetFromJsonAsAsyncEnumerable<Mod>(ModLinksUrl, cancellationToken);
            Logger.Information("Mods fetched from GitHub successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHub");
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