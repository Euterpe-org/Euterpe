using MuseDashModToolsUI.Views;

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
    }

    public void Button_FilterAll()
    {
        MainWindow._Instance.Selected_ModFilter = 0;
        MainWindow._Instance.UpdateFilters();
    }

    public void Button_FilterInstalled()
    {
        MainWindow._Instance.Selected_ModFilter = 1;
        MainWindow._Instance.UpdateFilters();
    }

    public void Button_FilterEnabled()
    {
        MainWindow._Instance.Selected_ModFilter = 2;
        MainWindow._Instance.UpdateFilters();
    }

    public void Button_FilterOutdated()
    {
        MainWindow._Instance.Selected_ModFilter = 3;
        MainWindow._Instance.UpdateFilters();
    }
}