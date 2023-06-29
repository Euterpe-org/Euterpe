using System.IO;
using System.Text.Json.Serialization;

namespace MuseDashModToolsUI.Models;

public class Setting
{
    public string? MuseDashFolder { get; set; }
    public string? LanguageCode { get; set; }
    public string? FontName { get; set; } = "Segoe UI";

    [JsonIgnore]
    public string UserDataFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "UserData") : string.Empty;

    [JsonIgnore]
    public string ModsFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "Mods") : string.Empty;

    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenInstalling { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenEnabling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDeleting { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDisabling { get; set; } = AskType.Always;

    public Setting Clone()
    {
        return (Setting)MemberwiseClone();
    }
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