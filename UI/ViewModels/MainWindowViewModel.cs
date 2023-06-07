using MuseDashModToolsUI.Contracts.ViewModels;

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IModManageViewModel _modManageViewModel;

    public MainWindowViewModel(IModManageViewModel modManageViewModel)
    {
        _modManageViewModel = modManageViewModel;
    }
}