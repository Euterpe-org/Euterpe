namespace MuseDashModTools.Models.GitHub;

public sealed class GitHubRelease
{
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

    [JsonPropertyName("assets_url")] public string AssetsUrl { get; set; } = string.Empty;

    [JsonPropertyName("upload_url")] public string UploadUrl { get; set; } = string.Empty;

    [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("node_id")] public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("tag_name")] public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("target_commitish")] public string TargetCommitish { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("draft")] public bool Draft { get; set; }

    [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }

    [JsonPropertyName("published_at")] public DateTime PublishedAt { get; set; }

    [JsonPropertyName("assets")] public GitHubReleaseAsset[] Assets { get; set; } = [];

    [JsonPropertyName("tarball_url")] public string TarballUrl { get; set; } = string.Empty;

    [JsonPropertyName("zipball_url")] public string ZipballUrl { get; set; } = string.Empty;

    [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
}