namespace MuseDashModTools.Models.GitHub;

[PublicAPI]
public sealed class GitHubRelease
{
    public string Url { get; set; } = string.Empty;
    public string AssetsUrl { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public int Id { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string TargetCommitish { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Draft { get; set; }
    public bool Prerelease { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PublishedAt { get; set; }
    public GitHubReleaseAsset[] Assets { get; set; } = [];
    public string TarballUrl { get; set; } = string.Empty;
    public string ZipballUrl { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}