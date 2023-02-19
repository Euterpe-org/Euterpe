using System.Reactive;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Views;
using ReactiveUI;

namespace MuseDashModToolsUI.ViewModels;

internal class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInstalledCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterEnabledCommand { get; }
    //TODO We inject the service here through the DI, you now can separate logic in Services, I've already made one for github, which will be very convenient 
    public MainWindowViewModel(IGitHubService service)
    {
        FilterAllCommand = ReactiveCommand.Create(FilterAll);
        FilterInstalledCommand = ReactiveCommand.Create(FilterInstalled);
        FilterEnabledCommand = ReactiveCommand.Create(FilterEnabled);
    }

    public void FilterAll()
    {
        MainWindow.Instance!.Selected_ModFilter = 0;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterInstalled()
    {
        MainWindow.Instance!.Selected_ModFilter = 1;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterEnabled()
    {
        MainWindow.Instance!.Selected_ModFilter = 2;
        MainWindow.Instance.UpdateFilters();
    }

    public void FilterOutdated()
    {
        MainWindow.Instance!.Selected_ModFilter = 3;
        MainWindow.Instance.UpdateFilters();
    }
}