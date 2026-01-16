using System.Net.Http.Json;
using IDownloadService = Euterpe.Abstractions.IDownloadService;
using static Euterpe.Core.JsonContexts.CamelCaseJsonContext;

namespace Euterpe.Core;

internal sealed class GiteeDownloadService : IGiteeDownloadService
{
    private const string GiteeBaseUrl = "https://gitee.com/";
    private const string RawModLinksUrl = GiteeBaseUrl + "lxymahatma/ModLinks/raw/" + ModLinksBranch;
    private const string ModJsonUrl = RawModLinksUrl + "Mods.json";
    private const string LibJsonUrl = RawModLinksUrl + "Libs.json";
    private const string ModsFolderUrl = RawModLinksUrl + "Mods/";
    private const string LibsFolderUrl = RawModLinksUrl + "Libs/";
    private const string ModToolsReleaseDownloadBaseUrl = GiteeBaseUrl + "lxymahatma/Euterpe/releases/download/";

    Task<bool> IDownloadService.DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<bool> IDownloadService.DownloadUnityDependencyAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<bool> IDownloadService.DownloadCpp2ILExecutableAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    Task<bool> IDownloadService.DownloadCpp2ILPluginAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public async Task DownloadReleaseByTagAsync(string tag, string osString, string updateFolder, CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"{ModToolsReleaseDownloadBaseUrl}{tag}/Euterpe-{osString}.zip";

        try
        {
            await Downloader.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(updateFolder, "Euterpe.zip"),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version from Gitee");
        }
    }

    Task<string?> IDownloadService.FetchReadmeAsync(string repoId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading mod {mod.Name} from Gitee...");

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
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download mod {mod.Name} from Gitee");
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading lib {lib.Name} from Gitee...");

        var downloadLink = LibsFolderUrl + lib.FileName;
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
            Logger.ZLogError(ex, $"Failed to download lib {lib.Name} from Gitee");
            return false;
        }
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching mod list from Gitee {ModJsonUrl} ...");

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(ModJsonUrl, Default.Mod, cancellationToken);
    }

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching lib list from Gitee {LibJsonUrl} ...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(LibJsonUrl, Default.Lib, cancellationToken);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required MultiThreadDownloader Downloader { get; init; }

    [UsedImplicitly]
    public required ILogger<GiteeDownloadService> Logger { get; init; }

    #endregion Injections
}