using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
    [ObservableProperty] private ViewModelBase _content;
    [ObservableProperty] private int _selectedIdx;
    [ObservableProperty] private ObservableCollection<TabView<ViewModelBase>> _tabs = new();

    public MainWindowViewModel()
    {
        Tabs = new ObservableCollection<TabView<ViewModelBase>>
        {
            new((ViewModelBase)_resolver.GetRequiredService<IModManageViewModel>(), XAML_Tab_ModManage, true),
            new((ViewModelBase)_resolver.GetRequiredService<ISettingsViewModel>(), XAML_Tab_Setting, false)
        };

        Content = Tabs[0].ViewModel;
    }

    [RelayCommand]
    private void SwitchTab()
    {
        Content = Tabs[SelectedIdx].ViewModel;
    }
}