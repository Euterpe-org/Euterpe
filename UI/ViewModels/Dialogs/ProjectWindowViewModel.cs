using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using Serilog;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Dialogs;

public partial class ProjectWindowViewModel : ViewModelBase, IProjectWindowViewModel
{
    public ILogger Logger { get; init; }
    public IDialogService DialogService { get; init; }

    [RelayCommand]
    private void Close()
    {
        DialogService.CloseDialog();
        Logger.Information("Close Project Window");
    }

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