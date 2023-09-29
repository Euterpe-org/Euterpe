using System.Diagnostics;

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class AboutViewModel : ViewModelBase, IAboutViewModel
{
    [UsedImplicitly]
    public ILogger? Logger { get; init; }

    [RelayCommand]
    private void OpenUrl(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
        Logger.Information("Open Url: {Url}", path);
    }
}