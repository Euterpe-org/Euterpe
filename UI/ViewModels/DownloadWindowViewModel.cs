using CommunityToolkit.Mvvm.ComponentModel;

namespace MuseDashModToolsUI.ViewModels;

public partial class DownloadWindowViewModel : ViewModelBase
{
    [ObservableProperty] private double _percentage;
    [ObservableProperty] private bool _downloadFinished;
}