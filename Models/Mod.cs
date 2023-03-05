using System.Text.Json.Serialization;

namespace MuseDashModToolsUI.Models;

public class Mod
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? FileName { get; set; }
    [JsonIgnore] public bool IsLocal => FileName is not null;
    [JsonIgnore] public bool IsDisabled { get; set; }
    [JsonIgnore] public bool IsTracked { get; set; }
    [JsonIgnore] public bool IsShaMismatched { get; set; }
    
    [JsonIgnore] public string XamlDescription => $"{Description} &#x0a; &#x0a; Author: {Author} &#x0a; Version: {Version}";
    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }
    public string[]? GameVersion { get; set; }
    public string? Description { get; set; }
    public string[]? DependentMods { get; set; }
    public string[]? DependentLibs { get; set; }
    [JsonIgnore] public bool HasDependencies => DependentLibs is not null && DependentLibs.Length > 0;
    
    public string[]? IncompatibleMods { get; set; }
    public string? SHA256 { get; set; }
    public string FileNameExtended(bool reverse = false)
    {
        return FileName + ((reverse ? !IsDisabled : IsDisabled) ? ".disabled" : "");
    }
}