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

    [ObservableProperty]
    public partial bool IsLocal { get; set; }

    [ObservableProperty]
    public partial bool IsInstallable { get; set; }

    [ObservableProperty]
    public partial bool IsReinstallable { get; set; }

    [ObservableProperty]
    public partial bool IsValidConfigFile { get; set; }

    public string[] DuplicatedModPaths { get; set; } = [];

    // GitHub Repo
    public string RepoPageUrl => GitHubBaseUrl + Repository;

    // Dependencies
    public bool HasDependency => ModDependencies.Length + LibDependencies.Length > 0;

    public string[] DependencyNames => !HasDependency ? [] : [..ModDependencies, ..LibDependencies];

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
    public string[] ModDependencies { get; set; } = [];
    public string[] LibDependencies { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;

    #endregion Mod Properties
}