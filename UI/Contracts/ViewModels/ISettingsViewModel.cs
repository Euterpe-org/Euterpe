namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface ISettingsViewModel
{
    string[] AskTypes { get; set; }
    string[] DownloadSources { get; set; }
    int CurrentDownloadSource { get; set; }
    int DisableDependenciesWhenDeleting { get; set; }
    int DisableDependenciesWhenDisabling { get; set; }
    int EnableDependenciesWhenEnabling { get; set; }
    int EnableDependenciesWhenInstalling { get; set; }
    void UpdatePath();
}