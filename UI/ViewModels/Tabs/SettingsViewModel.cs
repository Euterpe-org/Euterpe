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
    private readonly IFontManageService _fontManageService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;
    [ObservableProperty] private string[] _askTypes;
    [ObservableProperty] private int _currentDownloadSource;
    [ObservableProperty] private int _currentFontIndex;
    [ObservableProperty] private int _currentLanguageIndex;
    [ObservableProperty] private int _disableDependenciesWhenDeleting;
    [ObservableProperty] private int _disableDependenciesWhenDisabling;
    [ObservableProperty] private string[] _downloadSources;
    [ObservableProperty] private int _enableDependenciesWhenEnabling;
    [ObservableProperty] private int _enableDependenciesWhenInstalling;
    [ObservableProperty] private string? _path;
    public List<Language> AvailableLanguages => _localizationService.AvailableLanguages;
    public List<string> AvailableFonts => _fontManageService.AvailableFonts;

    public SettingsViewModel(IFontManageService fontManageService, ILocalizationService localizationService, ILogger logger,
        ISettingService settingService)
    {
        _fontManageService = fontManageService;
        _localizationService = localizationService;
        _logger = logger;
        _settingService = settingService;
        Initialize();
    }

    public void Initialize()
    {
        if (_settingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_settingService.Settings.LanguageCode);
        AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
        DownloadSources = new[]
            { XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee };
        CurrentLanguageIndex = AvailableLanguages.FindIndex(x => x.Name == CultureInfo.CurrentUICulture.Name);
        CurrentFontIndex = AvailableFonts.FindIndex(x => x == _settingService.Settings.FontName);
        Path = _settingService.Settings.MuseDashFolder;
        CurrentDownloadSource = (int)_settingService.Settings.DownloadSource;
        EnableDependenciesWhenInstalling = (int)_settingService.Settings.AskEnableDependenciesWhenInstalling;
        EnableDependenciesWhenEnabling = (int)_settingService.Settings.AskEnableDependenciesWhenEnabling;
        DisableDependenciesWhenDeleting = (int)_settingService.Settings.AskDisableDependenciesWhenDeleting;
        DisableDependenciesWhenDisabling = (int)_settingService.Settings.AskDisableDependenciesWhenDisabling;

        _logger.Information("Settings Window initialized");
    }

    [RelayCommand]
    private void SetLanguage() => _localizationService.SetLanguage(AvailableLanguages[CurrentLanguageIndex].Name!);

    [RelayCommand]
    private void SetFont() => _fontManageService.SetFont(AvailableFonts[CurrentFontIndex]);

    [RelayCommand]
    private async Task OnChoosePath()
    {
        _logger.Information("Choose path button clicked");
        await _settingService.OnChoosePath();
    }

    #region OnPropertyChanged

    partial void OnCurrentDownloadSourceChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            newValue = oldValue;
        else
            _settingService.Settings.DownloadSource = (DownloadSources)newValue;
    }

    partial void OnEnableDependenciesWhenInstallingChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            newValue = oldValue;
        else
            _settingService.Settings.AskEnableDependenciesWhenInstalling = (AskType)newValue;
    }

    partial void OnEnableDependenciesWhenEnablingChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            newValue = oldValue;
        else
            _settingService.Settings.AskEnableDependenciesWhenEnabling = (AskType)newValue;
    }

    partial void OnDisableDependenciesWhenDeletingChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            newValue = oldValue;
        else
            _settingService.Settings.AskDisableDependenciesWhenDeleting = (AskType)newValue;
    }

    partial void OnDisableDependenciesWhenDisablingChanged(int oldValue, int newValue)
    {
        if (newValue == -1)
            newValue = oldValue;
        else
            _settingService.Settings.AskDisableDependenciesWhenDisabling = (AskType)newValue;
    }

    #endregion
}