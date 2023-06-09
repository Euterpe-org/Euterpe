using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IModManageViewModel _modManageViewModel;
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;

    [ObservableProperty] private ObservableCollection<TabView<ViewModelBase>> _tabs = new();

    public MainWindowViewModel(IModManageViewModel modManageViewModel)
    {
        _modManageViewModel = modManageViewModel;

        Tabs = new ObservableCollection<TabView<ViewModelBase>>
        {
            new((ViewModelBase)_resolver.GetRequiredService<IModManageViewModel>(), XAML_Tab_ModManage, true),
            new((ViewModelBase)_resolver.GetRequiredService<ISettingsViewModel>(), XAML_Tab_Settings, false)
        };
    }
}