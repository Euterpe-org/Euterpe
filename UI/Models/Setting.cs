using System.IO;
using System.Text.Json.Serialization;

namespace MuseDashModToolsUI.Models;

public class Setting
{
    public string? MuseDashFolder { get; set; }
    [JsonIgnore] public string ModsFolder => !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "Mods") : string.Empty;
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenInstalling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDeleting { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenEnabling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDisabling { get; set; } = AskType.Always;
}

public enum AskType
{
    Always,
    YesAndNoAsk,
    NoAndNoAsk
}