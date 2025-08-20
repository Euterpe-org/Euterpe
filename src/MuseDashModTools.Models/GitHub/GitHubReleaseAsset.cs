namespace MuseDashModTools.Models.GitHub;

[PublicAPI]
public sealed class GitHubReleaseAsset
{
    public string Url { get; set; } = string.Empty;
    public int Id { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public object? Label { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Size { get; set; }
    public int DownloadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}