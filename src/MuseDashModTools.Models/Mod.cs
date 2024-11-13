namespace MuseDashModTools.Models;

[PublicAPI]
public sealed class Mod
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DownloadLink { get; set; } = string.Empty;
    public string RepositoryIdentifier { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string[] GameVersion { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string[] DependentMods { get; set; } = [];
    public string[] DependentLibs { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;
}