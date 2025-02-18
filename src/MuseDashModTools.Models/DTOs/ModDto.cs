namespace MuseDashModTools.Models;

public sealed partial class ModDto : ObservableObject
{
    public ModDto RemoveLocalInfo()
    {
        FileNameWithoutExtension = null;
        LocalVersion = string.Empty;
        SHA256 = string.Empty;
        return this;
    }

    #region Dto Properties

    // Local Information
    public string LocalVersion { get; set; } = string.Empty;
    public ModState State { get; set; }
    public string? FileNameWithoutExtension { get; set; }
    public string LocalFileName => FileNameWithoutExtension + (IsDisabled ? ".disabled" : ".dll");
    public string ReversedFileName => FileNameWithoutExtension + (IsDisabled ? ".dll" : ".disabled");

    // Binding Boolean Properties
    [ObservableProperty]
    public partial bool IsDisabled { get; set; } = true;

    public bool IsLocal => FileNameWithoutExtension is not null;
    public bool IsInstallable => !IsLocal && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is not (ModState.Normal or ModState.Newer);
    public string? DuplicatedModPaths { get; set; }

    public bool IsValidConfigFile { get; set; }

    // GitHub Repo
    public string RepoPageUrl => GitHubBaseUrl + Repository;

    public bool IsValidRepository => !RepoPageUrl.IsNullOrEmpty() && Uri.TryCreate(RepoPageUrl, UriKind.Absolute, out _);

    // Dependencies
    public bool HasDependency => DependentMods.Length + DependentLibs.Length > 0;

    public string[] DependencyNames => !HasDependency ? [] : [..DependentMods, ..DependentLibs];

    // Compatible
    public string CompatibleGameVersion => GameVersion == "*" ? XAML_Mod_CompatibleGameVersion : GameVersion;

    #endregion Dto Properties

    #region Mod Properties

    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "Unknown";
    public string Author { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string GameVersion { get; set; } = "Unknown";
    public string Description { get; set; } = string.Empty;
    public string[] DependentMods { get; set; } = [];
    public string[] DependentLibs { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;

    #endregion Mod Properties
}