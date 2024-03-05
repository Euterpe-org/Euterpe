namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface IDownloadWindowViewModel
{
    Setting Settings { get; }
    Task InstallMelonLoader();
}