using System.Net;

namespace MuseDashModTools.Services;

public sealed partial class GitHubMirrorDownloadService
{
    private async Task<string?> FetchReadmeFromBranchAsync(string repoId, string branch, CancellationToken cancellationToken = default)
    {
        var url = $"{PrimaryRawMirrorUrl}{repoId}/{branch}/README.md";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            Logger.Information("Successfully fetched Readme from branch {Branch} of {Repo} from GitHub", branch, repoId);
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch Readme from {Repo} on branch {Branch} from GitHub", repoId, branch);
            return null;
        }
    }

    private async Task<string?> FetchReadmeFromBranchesAsync(string repoId, CancellationToken cancellationToken)
    {
        var branches = new[] { "master", "main" };

        foreach (var branch in branches)
        {
            var readme = await FetchReadmeFromBranchAsync(repoId, branch, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(readme))
            {
                return readme;
            }
        }

        Logger.Information("No Readme found in branches for {Repo}", repoId);
        return null;
    }
}