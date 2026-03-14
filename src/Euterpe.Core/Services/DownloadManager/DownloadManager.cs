using System.Net.Http.Json;
using Euterpe.Models.Dependencies;
using static Euterpe.Core.JsonContexts.CamelCaseJsonContext;
using static Euterpe.Shared.EuterpeCdn;

namespace Euterpe.Core;

internal sealed partial class DownloadManager : IDownloadManager
{
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

    public Task<bool> DownloadDependencyAsync(
        DependencySpec spec,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading dependency {spec.Name} from {spec.Url} ...");

        return DownloadFileAsync(spec.Url, spec.FilePath, onDownloadStarted, progress, cancellationToken);
    }

    public async Task<bool> DownloadAssetAsync(string downloadUrl, string filePath, string displayName, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading {displayName} ...");

        try
        {
            var stream = await Client.GetStreamAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download {displayName}");
            return false;
        }
    }

    public async Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        var downloadLink = Assets.ModsBaseUrl + mod.FileName;
        var path = Path.Combine(Config.ModsFolder, mod.FileName);

        return await DownloadAssetAsync(downloadLink, path, $"mod {mod.Name}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        var downloadLink = Assets.LibsBaseUrl + lib.FileName;
        var path = Path.Combine(Config.UserLibsFolder, lib.FileName);

        return await DownloadAssetAsync(downloadLink, path, $"lib {lib.Name}", cancellationToken).ConfigureAwait(false);
    }

    public async Task DownloadReleaseByTagAsync(string tag, string runtimeIdentifier, string updateFolder, CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"{Releases.BaseUrl}{tag}/Euterpe-{runtimeIdentifier}.zip";

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

    public IAsyncEnumerable<Mod?> FetchModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching mods from GitHub {Assets.ModsJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Mod>(Assets.ModsJsonUrl, Default.Mod, cancellationToken);
    }

    public IAsyncEnumerable<Lib?> FetchLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs from GitHub {Assets.LibsJsonUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Lib>(Assets.LibsJsonUrl, Default.Lib, cancellationToken);
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