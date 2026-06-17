namespace Euterpe.Contracts.GitHub;

[PublicAPI]
public sealed class GitHubReadmeContent : GitHubRepoContent
{
    public string Content { get; set; } = string.Empty;
    public string Encoding { get; set; } = string.Empty;
}
