using Mapster;

namespace MuseDashModTools.Models;

public sealed class ModDto : ObservableObject
{
    // Mod Properties
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "Unknown";
    public string Author { get; set; } = string.Empty;
    public string DownloadLink { get; set; } = string.Empty;
    public string HomePage { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string[] GameVersion { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string[] DependentMods { get; set; } = [];
    public string[] DependentLibs { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;

    // Dto Properties
    public bool IsDisabled => FileExtension == ".disabled";
    public string LocalVersion { get; set; } = string.Empty;
    public ModState State { get; set; }
    public string FileName => FileNameWithoutExtension + FileExtension;
    public string? FileNameWithoutExtension { get; set; }
    public string? FileExtension { get; set; }
    public bool IsLocal => FileNameWithoutExtension is not null;
    public bool IsInstallable => !IsLocal && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is not (ModState.Normal or ModState.Newer);
    public bool IsDuplicated => State is ModState.Duplicated;
    public string? DuplicatedModPaths { get; set; }

    public string XamlDescription => string.Format(XAML_Mod_Description.NormalizeNewline(),
        ModDescriptionProvider.GetDescription(this), Author, Version, CompatibleGameVersion);

    public bool IsValidConfigFile => !ConfigFile.IsNullOrEmpty() && File.Exists(ConfigFile);

    public bool IsValidHomePage => !HomePage.IsNullOrEmpty() && Uri.TryCreate(HomePage, UriKind.Absolute, out var uriResult) &&
                                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    public string CompatibleGameVersion
    {
        get
        {
            if (GameVersion is [])
            {
                return "Unknown";
            }

            return GameVersion[0] == "*" ? XAML_Mod_CompatibleGameVersion : string.Join(", ", GameVersion);
        }
    }

    public bool HasDependency => DependentMods.Length + DependentLibs.Length > 0;

    public string DependencyNames => !HasDependency ? string.Empty : string.Join("\r\n", DependentMods.Concat(DependentLibs));

    public ModDto()
    {
    }

    public ModDto(Mod mod) => UpdateFromMod(mod);

    public ModDto RemoveLocalInfo()
    {
        FileNameWithoutExtension = null;
        LocalVersion = string.Empty;
        SHA256 = string.Empty;
        return this;
    }

    public void UpdateFromMod(Mod? mod) => mod.Adapt(this);
}