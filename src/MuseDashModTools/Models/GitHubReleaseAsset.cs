using System.Text.Json.Serialization;

namespace MuseDashModTools.Models;

public sealed class GitHubReleaseAsset
{
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("node_id")] public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("label")] public object? Label { get; set; }

    [JsonPropertyName("content_type")] public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;

    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("download_count")] public int DownloadCount { get; set; }

    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}