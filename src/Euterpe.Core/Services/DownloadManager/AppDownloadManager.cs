using System.Net;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace Euterpe.Core;

internal sealed class AppDownloadManager : IAppDownloadManager
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

    public async Task<bool> DownloadAssetAsync(string downloadUrl, string filePath, string displayName, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading {displayName} ...");

        try
        {
            var stream = await DownloadClient.GetStreamAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
            await using (stream.ConfigureAwait(false))
            {
                var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await using (fs.ConfigureAwait(false))
                {
                    await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download {displayName}");
            return false;
        }
    }

    public async Task<bool> DownloadReleaseAsync(string downloadUrl, string updateFolder, CancellationToken cancellationToken = default)
    {
        try
        {
            await DownloadService.DownloadFileTaskAsync(downloadUrl, Path.Combine(updateFolder, "Euterpe.zip"), cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download new version");
            return false;
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

    private async Task<string?> FetchReadmeFromBranchesAsync(string repoId, CancellationToken cancellationToken)
    {
        foreach (var branch in Branches)
        {
            var readme = await FetchReadmeFromBranchAsync(repoId, branch, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(readme))
            {
                return readme;
            }
        }

        Logger.ZLogInformation($"No Readme found in any branches for {repoId}");
        return null;
    }

    private async Task<string?> FetchReadmeFromBranchAsync(string repoId, string branch, CancellationToken cancellationToken)
    {
        foreach (var url in CommonReadmeNames.Select(readmeName => $"{GitHubRawContentBaseUrl}{repoId}/{branch}/{readmeName}"))
        {
            var content = await TryFetchContentAsync(url, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(content))
            {
                continue;
            }

            Logger.ZLogInformation($"Successfully fetched Readme from branch {branch} of {repoId} using URL: {url}");
            return content;
        }

        return null;
    }

    private async Task<string?> TryFetchContentAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch content from URL: {url}");
            return null;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required EuterpeDownloadClient DownloadClient { get; init; }

    [UsedImplicitly]
    public required IDownloadService DownloadService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppDownloadManager> Logger { get; init; }

    #endregion Injections
}