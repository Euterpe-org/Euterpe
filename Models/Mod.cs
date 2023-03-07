using System.Collections.Generic;
using System.Text.Json.Serialization;
using ReactiveUI;

namespace MuseDashModToolsUI.Models;

public class Mod : ReactiveObject
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? LocalVersion { get; set; }
    [JsonIgnore] public bool HasUpdate => Version is not null && LocalVersion is not null && Version != LocalVersion;
    public string? Author { get; set; }
    public string? FileName { get; set; }
    [JsonIgnore] public bool IsLocal => FileName is not null;
    [JsonIgnore] public bool IsDisabled { get; set; }
    [JsonIgnore] public bool IsTracked { get; set; }
    [JsonIgnore] public bool IsShaMismatched { get; set; }
    [JsonIgnore] public bool IsDuplicated { get; set; }
    
    [JsonIgnore] public string XamlDescription => $"{Description} \n\n Author: {Author} \n Version: {Version}";
    private bool _isExpanded;
    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }
    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }
    public string[]? GameVersion { get; set; }
    public string? Description { get; set; }
    public List<string> DependentMods { get; set; } = new();
    public List<string> DependentLibs { get; set; } = new();
    public List<string> IncompatibleMods { get; set; } = new();
    public string? SHA256 { get; set; }
    public string FileNameExtended(bool reverse = false)
    {
        return FileName + ((reverse ? !IsDisabled : IsDisabled) ? ".disabled" : "");
    }
}