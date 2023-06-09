using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
    [ObservableProperty] private Control? _content;
    private int _selectedTabIndex;
    [ObservableProperty] private ObservableCollection<TabView> _tabs = new();

    public MainWindowViewModel()
    {
        Tabs = new ObservableCollection<TabView>
        {
            new((ViewModelBase)_resolver.GetRequiredService<IModManageViewModel>(), XAML_Tab_ModManage),
            new((ViewModelBase)_resolver.GetRequiredService<ISettingsViewModel>(), XAML_Tab_Setting)
        };
    }

    public void SwitchTab(int index)
    {
        Content = Tabs[index].View;
        _selectedTabIndex = index;
    }


    public void Refresh()
    {
        Tabs[0].DisplayName = XAML_Tab_ModManage;
        Tabs[1].DisplayName = XAML_Tab_Setting;
        Content = Tabs[_selectedTabIndex].View;
    }
}