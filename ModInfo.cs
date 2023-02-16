namespace MuseDashModToolsUI;

public class WebModInfo
{
    internal string? Name { get; set; }
    internal string? Version { get; set; }
    internal string? Author { get; set; }
    internal string? DownloadLink { get; set; }
    internal string? HomePage { get; set; }
    internal string[]? GameVersion { get; set; }
    internal string? Description { get; set; }
    internal string[]? DependentMods { get; set; }
    internal string[]? DependentLibs { get; set; }
    internal string[]? IncompatibleMods { get; set; }
    internal string? SHA256 { get; set; }
}

public class LocalModInfo
{
    internal string? Name { get; set; }
    internal string? Version { get; set; }
    internal string? SHA256 { get; set; }
    internal string? FileName { get; set; }
    internal bool Disabled { get; set; }
    internal string? Description { get; set; }
    internal string? Author { get; set; }
    internal string? HomePage { get; set; }
}