using MuseDashModToolsUI.Views;

namespace MuseDashModToolsUI.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    internal MainWindowViewModel()
    {
    }

    internal void Button_FilterAll()
    {
        MainWindow.Instance!.Selected_ModFilter = 0;
        MainWindow.Instance.UpdateFilters();
    }

    internal void Button_FilterInstalled()
    {
        MainWindow.Instance!.Selected_ModFilter = 1;
        MainWindow.Instance.UpdateFilters();
    }

    internal void Button_FilterEnabled()
    {
        MainWindow.Instance!.Selected_ModFilter = 2;
        MainWindow.Instance.UpdateFilters();
    }

    internal void Button_FilterOutdated()
    {
        MainWindow.Instance!.Selected_ModFilter = 3;
        MainWindow.Instance.UpdateFilters();
    }
}