using System.Diagnostics;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MuseDashModToolsUI.ViewModels.Pages;

public sealed partial class AboutViewModel : ViewModelBase, IAboutViewModel
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