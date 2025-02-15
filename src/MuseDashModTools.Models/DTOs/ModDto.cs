namespace MuseDashModTools.Models;

public sealed class ModDto : ObservableObject
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
    public string FileName => FileNameWithoutExtension + FileExtension;
    public string? FileNameWithoutExtension { get; set; }
    public string? FileExtension { get; set; }

    // Binding Boolean Properties
    public bool IsDisabled => !IsLocal || FileExtension == ".disabled";
    public bool IsLocal => FileNameWithoutExtension is not null;
    public bool IsInstallable => !IsLocal && State is not ModState.Incompatible;
    public bool IsReinstallable => IsLocal && State is not (ModState.Normal or ModState.Newer);
    public bool IsDuplicated => State is ModState.Duplicated;
    public string? DuplicatedModPaths { get; set; }

    public bool IsValidConfigFile { get; set; }

    // GitHub Repo
    public string RepoPageUrl => GitHubBaseUrl + RepositoryIdentifier;

    public bool IsValidRepository => !RepoPageUrl.IsNullOrEmpty() && Uri.TryCreate(RepoPageUrl, UriKind.Absolute, out _);

    // Dependencies
    public bool HasDependency => DependentMods.Length + DependentLibs.Length > 0;

    public string[] DependencyNames => !HasDependency ? [] : [..DependentMods, ..DependentLibs];

    // Compatible
    public string CompatibleGameVersions => GameVersion == "*" ? XAML_Mod_CompatibleGameVersion : GameVersion;

    #endregion Dto Properties

    #region Mod Properties

    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "Unknown";
    public string Author { get; set; } = string.Empty;
    public string DownloadLink { get; set; } = string.Empty;
    public string RepositoryIdentifier { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] DependentMods { get; set; } = [];
    public string[] DependentLibs { get; set; } = [];
    public string[] IncompatibleMods { get; set; } = [];
    public string SHA256 { get; set; } = string.Empty;

    #endregion Mod Properties
}