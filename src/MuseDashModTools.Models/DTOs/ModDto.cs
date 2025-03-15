using MuseDashModTools.Localization;

namespace MuseDashModTools.Models;

public sealed partial class ModDto : ObservableObject
{
    public void RemoveLocalInfo()
    {
        LocalVersion = string.Empty;
        State = ModState.Normal;
        FileNameWithoutExtension = null;
        IsDisabled = true;
    }

    public void AddLocalInfo()
    {
        LocalVersion = Version;
        State = ModState.Normal;
        FileNameWithoutExtension = FileName[..^4];
        IsDisabled = false;
    }

    #region Dto Properties

    // Local Information
    [ObservableProperty]
    public partial string LocalVersion { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInstallable))]
    [NotifyPropertyChangedFor(nameof(IsReinstallable))]
    public partial ModState State { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocal))]
    [NotifyPropertyChangedFor(nameof(IsInstallable))]
    [NotifyPropertyChangedFor(nameof(IsReinstallable))]
    public partial string? FileNameWithoutExtension { get; set; }

    public string LocalFileName => FileNameWithoutExtension + (IsDisabled ? ".disabled" : ".dll");
    public string ReversedFileName => FileNameWithoutExtension + (IsDisabled ? ".dll" : ".disabled");

    // Binding Boolean Properties
    [ObservableProperty]
    public partial bool IsDisabled { get; set; } = true;

    public bool IsLocal => FileNameWithoutExtension is not null;
    public bool IsInstallable => !IsLocal && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is ModState.Modified;

    [ObservableProperty]
    public partial bool IsValidConfigFile { get; set; }

    public string[] DuplicatedModPaths { get; set; } = [];

    // GitHub Repo
    public string RepoPageUrl => GitHubBaseUrl + Repository;

    // Dependencies
    public bool HasDependency => ModDependencies.Length + LibDependencies.Length > 0;

    public string[] DependencyNames => !HasDependency ? [] : [..ModDependencies, ..LibDependencies];

    // LocalizedStrings
    public LocalizedString LocalizedCompatibleGameVersion => GameVersion == "*" ? AllGameVersionCompatible : GameVersion;

    public ModDescriptionLiteral.LocalizedString LocalizedModDescription =>
        ModDescription.ResourceManager.GetString(Name) is null ? Description : new ModDescriptionLiteral.LocalizedString(Name);

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