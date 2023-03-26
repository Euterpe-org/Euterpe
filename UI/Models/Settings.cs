using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Models;

public class Settings : ISettings
{
    public string? MuseDashFolder { get; set; }
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenInstalling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDeleting { get; set; } = AskType.Always;
    public AskType AskEnableDependenciesWhenEnabling { get; set; } = AskType.Always;
    public AskType AskDisableDependenciesWhenDisabling { get; set; } = AskType.Always;

    public Settings()
    {
    }
}

public enum AskType
{
    Always,
    YesAndNoAsk,
    NoAndNoAsk
}