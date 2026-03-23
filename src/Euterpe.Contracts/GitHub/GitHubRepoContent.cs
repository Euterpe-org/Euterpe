namespace Euterpe.Contracts.GitHub;

[PublicAPI]
public class GitHubRepoContent
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    [JsonPropertyName("sha")] public string SHA { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string GitUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("_links")] public GitHubRepoContentLinks Links { get; set; } = new();
}