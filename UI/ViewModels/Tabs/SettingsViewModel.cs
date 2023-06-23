using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    [ObservableProperty] private string[] _askTypes = { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
    [ObservableProperty] private Language? _currentLanguage;
    [ObservableProperty] private int _disableDependenciesWhenDeleting;
    [ObservableProperty] private int _disableDependenciesWhenDisabling;
    [ObservableProperty] private int _enableDependenciesWhenEnabling;
    [ObservableProperty] private int _enableDependenciesWhenInstalling;
    [ObservableProperty] private string? _path;
    public ILocalizationService LocalizationService { get; init; }
    public IModManageViewModel ModManageViewModel { get; init; }
    public List<Language> AvailableLanguages => LocalizationService.AvailableLanguages;

    public SettingsViewModel(ISettingService settingService, ILogger logger)
    {
        _settingService = settingService;
        _logger = logger;
        Initialize();
    }

    public void Initialize()
    {
        CurrentLanguage = _settingService.Settings.LanguageCode is null
            ? new Language(CultureInfo.CurrentUICulture)
            : new Language(CultureInfo.GetCultureInfo(_settingService.Settings.LanguageCode));
        Path = _settingService.Settings.MuseDashFolder;
        _logger.Information("Settings Window initialized");
    }

    [RelayCommand]
    private void SetLanguage() => LocalizationService.SetLanguage(CurrentLanguage!.Name!);

    [RelayCommand]
    private async Task OnChoosePath()
    {
        _logger.Information("Choose path button clicked");
        var changed = await _settingService.OnChoosePath();
        if (changed) await ModManageViewModel.Initialize();
    }

    #region OnPropertyChanged

    partial void OnEnableDependenciesWhenInstallingChanged(int oldValue, int newValue)
    {
        switch (newValue)
        {
            case -1:
                EnableDependenciesWhenInstalling = oldValue;
                break;
            case 0:
                _settingService.Settings.AskEnableDependenciesWhenInstalling = AskType.Always;
                break;
            case 1:
                _settingService.Settings.AskEnableDependenciesWhenInstalling = AskType.YesAndNoAsk;
                break;
            case 2:
                _settingService.Settings.AskEnableDependenciesWhenInstalling = AskType.NoAndNoAsk;
                break;
        }
    }

    #endregion
}