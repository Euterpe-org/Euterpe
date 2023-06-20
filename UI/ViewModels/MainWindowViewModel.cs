using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;
using ILogger = Serilog.ILogger;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly ILogger _logger;
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
    [ObservableProperty] private ViewModelBase? _content;
    [ObservableProperty] private List<TabView> _tabs = new();
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)!;

    public MainWindowViewModel(ILogger logger, ISettingService settingService)
    {
        _logger = logger;
        if (settingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settingService.Settings.LanguageCode);
        Tabs = new List<TabView>
        {
            new((ViewModelBase)_resolver.GetRequiredService<IModManageViewModel>(), XAML_Tab_ModManage),
            new((ViewModelBase)_resolver.GetRequiredService<ISettingsViewModel>(), XAML_Tab_Setting)
        };
        _logger.Information("MainWindow initialized");
    }

    public void SwitchTab(int index)
    {
        Content = Tabs[index].ViewModel;
        var name = Tabs[index].DisplayName;
        _logger.Information("Switching Tabs to {Name}", name);
    }

    public void ChangeTabName()
    {
        Tabs[0].DisplayName = XAML_Tab_ModManage;
        Tabs[1].DisplayName = XAML_Tab_Setting;
    }
}