using System.Net;

namespace Euterpe.Core;

internal sealed class AppDownloadManager : IAppDownloadManager
{
    public async Task DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        var downloadService = DownloadServiceFactory();
        await using (downloadService.ConfigureAwait(false))
        {
            if (onDownloadStarted is not null)
            {
                downloadService.DownloadStarted += onDownloadStarted;
            }

            if (downloadProgress is not null)
            {
                downloadService.DownloadProgressChanged += (_, e) => downloadProgress.Report(e.ProgressPercentage);
            }

            await downloadService.DownloadFileOrThrowAsync(url, filePath, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DownloadAssetAsync(string downloadUrl, string filePath, string displayName, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Downloading {DisplayName} ...", displayName);

        var stream = await DownloadClient.GetStreamAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        if (ReadmeCache.TryGetValue(repoId, out var readme))
        {
            Logger.LogInformation("Using cached Readme for {RepoId}", repoId);
            return readme;
        }

        Logger.LogInformation("Attempting to fetch Readme for {RepoId}", repoId);
        readme = await FetchReadmeFromBranchesAsync(repoId, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(readme))
        {
            ReadmeCache[repoId] = readme;
            return readme;
        }

        Logger.LogInformation("Branch readme fetch failed");
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

        Logger.LogInformation("No Readme found in any branches for {RepoId}", repoId);
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

            Logger.LogInformation("Successfully fetched Readme from branch {Branch} of {RepoId} using URL: {Url}", branch, repoId, url);
            return content;
        }

        return null;
    }

    private async Task<string?> TryFetchContentAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to fetch content from URL: {Url}", url);
            return null;
        }
    }

    #region Injections

    public required HttpClient Client { get; init; }
    public required EuterpeDownloadClient DownloadClient { get; init; }
    public required Func<DownloadService> DownloadServiceFactory { get; init; }
    public required ILogger<AppDownloadManager> Logger { get; init; }

    #endregion Injections
}
