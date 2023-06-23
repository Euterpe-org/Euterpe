using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly ISettingService _settingService;
    [ObservableProperty] private ViewModelBase? _content;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private List<TabView> _tabs = new();
    public ILogger? Logger { get; init; }
    public IGitHubService? GitHubService { get; init; }
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)!;

    public MainWindowViewModel(ISettingService settingService, ISettingsViewModel settingsViewModel, IModManageViewModel modManageViewModel)
    {
        _settingService = settingService;
        if (settingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settingService.Settings.LanguageCode);

        Tabs = new List<TabView>
        {
            new((ViewModelBase)modManageViewModel, XAML_Tab_ModManage, "ModManage"),
            new((ViewModelBase)settingsViewModel, XAML_Tab_Setting, "Setting")
        };

        Task.Run(() =>
        {
            SwitchTab();
            GitHubService?.CheckUpdates();
            Logger?.Information("Main Window initialized");
        });

        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    [RelayCommand]
    private void SwitchTab()
    {
        Content = Tabs[SelectedTabIndex].ViewModel;
        var name = Tabs[SelectedTabIndex].Name;
        Logger?.Information("Switching tab to {Name}", name);
    }

    private void OnExit(object sender, EventArgs e)
    {
        var json = JsonSerializer.Serialize(_settingService.Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("Settings.json", json);
        Logger?.Information("Settings saved");
    }
}