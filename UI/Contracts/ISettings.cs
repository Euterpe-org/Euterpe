using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ISettings
{
    public string? MuseDashFolder { get; set; }
    public AskType AskInstallMuseDashModTools { get; set; }
    public AskType AskEnableDependenciesWhenInstalling { get; set; }
    public AskType AskDisableDependenciesWhenDeleting { get; set; }
    public AskType AskEnableDependenciesWhenEnabling { get; set; }
    public AskType AskDisableDependenciesWhenDisabling { get; set; }
}