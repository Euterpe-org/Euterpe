namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface ISettingsViewModel
{
    string[] AskTypes { get; set; }
    string[] DownloadSources { get; set; }
    void Initialize();
}