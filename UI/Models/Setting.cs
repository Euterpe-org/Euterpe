using System;
using System.IO;
using Newtonsoft.Json;

namespace MuseDashModToolsUI.Models;

public class Setting
{
    public string? MuseDashFolder { get; set; }
    public string? LanguageCode { get; set; }
    public string? FontName { get; set; } = "Segoe UI";
    public Version SkipVersion { get; set; } = new();

    public bool DownloadPrerelease { get; set; } = false;
    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenInstalling { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenEnabling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDeleting { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDisabling { get; set; } = AskType.Always;

    [JsonIgnore]
    public string UserDataFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "UserData") : string.Empty;

    [JsonIgnore]
    public string ModsFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "Mods") : string.Empty;

    [JsonIgnore]
    public string MelonLoaderFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "MelonLoader") : string.Empty;

    public Setting Clone() => (Setting)MemberwiseClone();
}

public enum AskType
{
    Always,
    YesAndNoAsk,
    NoAndNoAsk
}

public enum DownloadSources
{
    Github,
    GithubMirror,
    Gitee
}