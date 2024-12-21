using System.Net.Http.Json;
using System.Text;

namespace MuseDashModTools.Services;

public abstract class GitHubServiceBase
{
    protected readonly Dictionary<string, string> _readmeUrlCache = [];

    public abstract HttpClient Client { get; init; }

    public abstract ILogger Logger { get; init; }

    protected async Task<string?> FetchReadmeFromApiAsync(string repoId, CancellationToken cancellationToken = default)
    {
        var url = $"{GitHubAPIBaseUrl}{repoId}/readme";

        try
        {
            var content = await Client.GetFromJsonAsync<ReadmeContent>(url, cancellationToken).ConfigureAwait(false);
            if (content is null)
            {
                return null;
            }

            Logger.Information("Successfully fetched Readme from API for {Repo}", repoId);
            return Encoding.UTF8.GetString(Convert.FromBase64String(content.Content));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch Readme from {Repo} using GitHub API", repoId);
            return null;
        }
    }
}