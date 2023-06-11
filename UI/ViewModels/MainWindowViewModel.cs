using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
    [ObservableProperty] private ViewModelBase? _content;
    [ObservableProperty] private List<TabView> _tabs = new();
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)!;

    public MainWindowViewModel(ISettingService settingService)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settingService.Settings.Language!);
        Tabs = new List<TabView>
        {
            new((ViewModelBase)_resolver.GetRequiredService<IModManageViewModel>(), XAML_Tab_ModManage),
            new((ViewModelBase)_resolver.GetRequiredService<ISettingsViewModel>(), XAML_Tab_Setting)
        };
    }

    public void SwitchTab(int index)
    {
        Content = Tabs[index].ViewModel;
    }

    public void ChangeTabName()
    {
        Tabs[0].DisplayName = XAML_Tab_ModManage;
        Tabs[1].DisplayName = XAML_Tab_Setting;
    }
}