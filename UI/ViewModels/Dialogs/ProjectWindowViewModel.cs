using System.Diagnostics;

#pragma warning disable CS8602

namespace MuseDashModToolsUI.ViewModels.Dialogs;

public partial class ProjectWindowViewModel : ViewModelBase, IProjectWindowViewModel
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