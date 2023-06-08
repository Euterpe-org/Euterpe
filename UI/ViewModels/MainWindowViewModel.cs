using System.Collections.ObjectModel;
using MuseDashModToolsUI.Contracts.ViewModels;

namespace MuseDashModToolsUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IModManageViewModel _modManageViewModel;
    public ObservableCollection<ViewModelBase> viewModels { get; set; } = new();

    public MainWindowViewModel(IModManageViewModel modManageViewModel)
    {
        _modManageViewModel = modManageViewModel;
    }
}