using System.Net;

namespace MuseDashModTools.Services;

public sealed partial class GitHubDownloadService
{
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
            Logger.Error(ex, "Failed to fetch content from URL: {Url}", url);
            return null;
        }
    }

    private async Task<string?> FetchReadmeFromBranchAsync(string repoId, string branch, CancellationToken cancellationToken = default)
    {
        foreach (var url in CommonReadmeNames.Select(readmeName => $"{GitHubRawContentBaseUrl}{repoId}/{branch}/{readmeName}"))
        {
            var content = await TryFetchContentAsync(url, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(content))
            {
                continue;
            }

            Logger.Information("Successfully fetched Readme from branch {Branch} of {Repo} using URL: {Url}", branch, repoId, url);
            return content;
        }

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

        Logger.Information("No Readme found in any branches for {Repo}", repoId);
        return null;
    }
}