namespace MuseDashModToolsUI;

public class WebModInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }
    public string[]? GameVersion { get; set; }
    public string? Description { get; set; }
    public string[]? DependentMods { get; set; }
    public string[]? DependentLibs { get; set; }
    public string[]? IncompatibleMods { get; set; }
    public string? SHA256 { get; set; }
}

public class LocalModInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? SHA256 { get; set; }
    public string? FileName { get; set; }
    public bool Disabled { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? HomePage { get; set; }
}