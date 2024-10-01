using System.Text.Json.Serialization;

namespace MuseDashModTools.Models.GitHub;

public sealed class GitHubRepoContentLinks
{
    [JsonPropertyName("self")] public string Self { get; set; } = string.Empty;
    [JsonPropertyName("git")] public string Git { get; set; } = string.Empty;
    [JsonPropertyName("html")] public string Html { get; set; } = string.Empty;
}