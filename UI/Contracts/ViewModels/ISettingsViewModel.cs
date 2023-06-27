namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface ISettingsViewModel
{
    string[] AskTypes { get; set; }
    string[] DownloadSources { get; set; }
    int CurrentDownloadSource { get; set; }
    void Initialize();
}