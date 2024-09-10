using Mapster;

namespace MuseDashModToolsUI.Models.DTOs;

public sealed class ModDto : ObservableObject
{
    // Mod Properties
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "Unknown";
    public string Author { get; set; } = string.Empty;
    public string DownloadLink { get; set; } = string.Empty;
    public string HomePage { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string[]? GameVersion { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string[] DependentMods { get; set; } = [];
    public string[] DependentLibs { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;

    // Dto Properties
    public bool IsDisabled { get; set; }
    public string LocalVersion { get; set; } = string.Empty;
    public ModState State { get; set; }

    public string? FileName { get; set; }

    public bool IsLocal => FileName is not null;

    public bool IsInstallable => !IsLocal && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is not (ModState.Normal or ModState.Newer);
    public bool IsTracked { get; set; }
    public bool IsDuplicated { get; set; }
    public string DuplicatedModNames { get; set; } = string.Empty;

    public string XamlDescription => string.Format(XAML_Mod_Description.NormalizeNewline(),
        ModDescriptionProvider.GetDescription(this), Author, Version, CompatibleGameVersion);

    public bool IsValidConfigFile { get; set; }

    public bool IsValidHomePage => !HomePage.IsNullOrEmpty() && Uri.TryCreate(HomePage, UriKind.Absolute, out var uriResult) &&
                                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public string CompatibleGameVersion
    {
        get
        {
            if (GameVersion is null or [])
            {
                return "Unknown";
            }

            return GameVersion[0] == "*" ? XAML_Mod_CompatibleGameVersion : string.Join(", ", GameVersion);
        }
    }

    public bool HasDependency => DependentMods.Length + DependentLibs.Length > 0;

    public string DependencyNames => !HasDependency ? string.Empty : string.Join("\r\n", DependentMods.Concat(DependentLibs));

    public void CloneOnlineInfo(Mod webMod)
    {
        DownloadLink = webMod.DownloadLink;
        ConfigFile = webMod.ConfigFile;
        HomePage = webMod.HomePage;
        GameVersion = webMod.GameVersion;
        Description = webMod.Description;
        DependentMods = webMod.DependentMods;
        DependentLibs = webMod.DependentLibs;
        IncompatibleMods = webMod.IncompatibleMods;
    }

    public ModDto RemoveLocalInfo()
    {
        FileName = null;
        IsValidConfigFile = false;
        IsDuplicated = false;
        IsTracked = false;
        LocalVersion = string.Empty;
        SHA256 = string.Empty;
        return this;
    }

    public void Apply(Mod mod) => mod.Adapt(this);
}