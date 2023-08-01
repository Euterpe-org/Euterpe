using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Newtonsoft.Json;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    [ObservableProperty] private ViewModelBase _content;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private List<TabView> _tabs = new();
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)!;

    public MainWindowViewModel(IGitHubService gitHubService, ILogger logger, ILogAnalysisViewModel logAnalysisViewModel,
        ISettingService settingService, ISettingsViewModel settingsViewModel, IModManageViewModel modManageViewModel)
    {
        _logger = logger;
        _settingService = settingService;

        Tabs = new List<TabView>
        {
            new((ViewModelBase)modManageViewModel, XAML_Tab_ModManage, "ModManage"),
            new((ViewModelBase)logAnalysisViewModel, XAML_Tab_LogAnalysis, "LogAnalysis"),
            new((ViewModelBase)settingsViewModel, XAML_Tab_Setting, "Setting")
        };
        SwitchTab();
        gitHubService.CheckUpdates();
        _logger.Information("Main Window initialized");
        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    [RelayCommand]
    private void SwitchTab()
    {
        Content = Tabs[SelectedTabIndex].ViewModel;
        var name = Tabs[SelectedTabIndex].Name;
        _logger.Information("Switching tab to {Name}", name);
    }

    private void OnExit(object sender, EventArgs e)
    {
        var json = JsonConvert.SerializeObject(_settingService.Settings, Formatting.Indented);
        File.WriteAllText("Settings.json", json);
        _logger.Information("Settings saved");
    }
}