using System.Net.Http.Json;
using static Euterpe.Core.JsonContexts.CamelCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class DownloadManager : IDownloadManager
{
    // Dependency URLs
    private const string MelonLoaderUrl = $"{DependenciesBaseUrl}/MelonLoader/{MelonLoaderVersion}/MelonLoader.x64.zip";
    private const string UnityDependencyUrl = $"{DependenciesBaseUrl}/UnityDependencies/{UnityDependencyVersion}/Managed.zip";
    private const string Cpp2ILExecutableUrl = $"{DependenciesBaseUrl}/Cpp2IL/{Cpp2ILVersion}/Cpp2IL-{Cpp2ILVersion}-Windows.exe";
    private const string Cpp2ILPluginUrl = $"{DependenciesBaseUrl}/Cpp2IL/{Cpp2ILVersion}/Cpp2IL.Plugin.StrippedCodeRegSupport.dll";

    // Mod URLs
    private const string ModJsonUrl = $"{AssetsBaseUrl}Mods.json";
    private const string LibJsonUrl = $"{AssetsBaseUrl}Libs.json";
    private const string ModsFolderUrl = $"{AssetsBaseUrl}Mods/";
    private const string LibsFolderUrl = $"{AssetsBaseUrl}Libs/";

    public async Task<bool> DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        EventHandler<DownloadProgressChangedEventArgs>? progressHandler = null;

        if (onDownloadStarted is not null)
        {
            DownloadService.DownloadStarted += onDownloadStarted;
        }

        if (downloadProgress is not null)
        {
            progressHandler = (_, e) => downloadProgress.Report(e.ProgressPercentage);
            DownloadService.DownloadProgressChanged += progressHandler;
        }

        try
        {
            await DownloadService.DownloadFileTaskAsync(url, filePath, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download file from {url} to {filePath}");
            return false;
        }
        finally
        {
            if (onDownloadStarted is not null)
            {
                DownloadService.DownloadStarted -= onDownloadStarted;
            }

            if (progressHandler is not null)
            {
                DownloadService.DownloadProgressChanged -= progressHandler;
            }
        }
    }

    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default) =>
        DownloadAsync(MelonLoaderUrl, Config.MelonLoaderZipPath, "MelonLoader", onDownloadStarted, onDownloadProgressChanged, cancellationToken);

    public Task<bool> DownloadUnityDependencyAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default) =>
        DownloadAsync(UnityDependencyUrl, Config.UnityDependencyZipPath, "Unity Dependency", onDownloadStarted, onDownloadProgressChanged, cancellationToken);

    public Task<bool> DownloadCpp2ILExecutableAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default) =>
        DownloadAsync(Cpp2ILExecutableUrl, Config.Cpp2ILExecutablePath, "Cpp2IL", onDownloadStarted, onDownloadProgressChanged, cancellationToken);

    public Task<bool> DownloadCpp2ILPluginAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default) =>
        DownloadAsync(Cpp2ILPluginUrl, Config.Cpp2ILPluginPath, "Cpp2IL Plugin", onDownloadStarted, onDownloadProgressChanged, cancellationToken);

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading mod {mod.Name} ...");

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
            Logger.ZLogError(ex, $"Failed to download mod {mod.Name}");
            return false;
        }
    }

    public async Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading lib {lib.Name} ...");

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
            Logger.ZLogError(ex, $"Failed to download lib {lib.Name}");
            return false;
        }
    }

    public async Task DownloadReleaseByTagAsync(string tag, string runtimeIdentifier, string updateFolder, CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"{ReleasesBaseUrl}{tag}/Euterpe-{runtimeIdentifier}.zip";

        try
        {
            await DownloadService.DownloadFileTaskAsync(downloadUrl,
                Path.Combine(updateFolder, "Euterpe.zip"),
                cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version");
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

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(ModJsonUrl, Default.Mod, cancellationToken);
    }

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs from GitHub {LibJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(LibJsonUrl, Default.Lib, cancellationToken);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required IDownloadService DownloadService { get; init; }

    [UsedImplicitly]
    public required ILogger<DownloadManager> Logger { get; init; }

    #endregion Injections
}