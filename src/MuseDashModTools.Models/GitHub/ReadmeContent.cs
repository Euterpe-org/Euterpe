namespace MuseDashModTools.Models.GitHub;

public sealed class ReadmeContent : GitHubRepoContent
{
    [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
    [JsonPropertyName("encoding")] public string Encoding { get; set; } = string.Empty;
}