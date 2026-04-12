namespace Euterpe.Models;

[PublicAPI]
public sealed class Mod
{
    public string Mid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string MelonVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] ModDependencies { get; set; } = [];
    public string[] LibDependencies { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];

    [JsonPropertyName("sha256")]
    public string SHA256 { get; set; } = string.Empty;

    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("download_count_total")]
    public string DownloadCount { get; set; } = string.Empty;
}