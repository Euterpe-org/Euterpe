using System.Text.Json.Serialization;

namespace MuseDashModToolsUI.Models;

public partial class Mod : ObservableObject
{
    [ObservableProperty] private bool _isDisabled;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    [JsonIgnore] public string? LocalVersion { get; set; }
    [JsonIgnore] public UpdateState State { get; set; }
    [JsonIgnore] public bool IsIncompatible { get; set; }
    [JsonIgnore] public bool IsUpdatable => IsLocal && State != UpdateState.Normal;
    public string Author { get; set; } = string.Empty;
    [JsonIgnore] public string? FileName { get; set; }
    [JsonIgnore] public bool IsLocal => FileName is not null;
    [JsonIgnore] public bool IsInstallable => !IsLocal && !IsIncompatible;
    [JsonIgnore] public bool IsTracked { get; set; }
    [JsonIgnore] public bool IsShaMismatched { get; set; }
    [JsonIgnore] public bool IsDuplicated { get; set; }
    [JsonIgnore] public string? DuplicatedModNames { get; set; }

    [JsonIgnore]
    public string XamlDescription => string.Format(XAML_Mod_Description.NormalizeNewline(), Description, Author, Version, CompatibleGameVersion);

    public string? DownloadLink { get; set; }
    public string? HomePage { get; set; }

    [JsonIgnore]
    public bool IsValidHomePage => HomePage is not null && Uri.TryCreate(HomePage, UriKind.Absolute, out var uriResult) &&
                                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public string[]? GameVersion { get; set; }

    [JsonIgnore]
    public string CompatibleGameVersion
    {
        get
        {
            if (GameVersion is null) return string.Empty;
            return GameVersion[0] == "*" ? XAML_Mod_CompatibleGameVersion : string.Join(", ", GameVersion);
        }
    }

    public string? Description { get; set; }
    public List<string> DependentMods { get; set; } = new();
    public List<string> DependentLibs { get; set; } = new();
    [JsonIgnore] public int DependencyCount => DependentMods.Count + DependentLibs.Count;

    [JsonIgnore]
    public string DependencyNames => DependencyCount == 0 ? string.Empty : string.Join("\r\n", DependentMods.Concat(DependentLibs));

    public List<string> IncompatibleMods { get; set; } = new();
    public string? SHA256 { get; set; }
    public string FileNameExtended(bool reverse = false) => FileName + ((reverse ? !IsDisabled : IsDisabled) ? ".disabled" : string.Empty);

    public void CloneOnlineInfo(Mod webMod)
    {
        DownloadLink = webMod.DownloadLink;
        HomePage = webMod.HomePage;
        GameVersion = webMod.GameVersion;
        Description = webMod.Description;
        DependentMods = webMod.DependentMods;
        DependentLibs = webMod.DependentLibs;
        IncompatibleMods = webMod.IncompatibleMods;
    }

    public Mod RemoveLocalInfo()
    {
        FileName = null;
        IsDisabled = false;
        IsDuplicated = false;
        IsIncompatible = false;
        IsShaMismatched = false;
        IsTracked = false;
        LocalVersion = null;
        SHA256 = null;
        return this;
    }
}