using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly IModManageViewModel _modManageViewModel;
    private readonly ISettingService _settingService;
    [ObservableProperty] private string[] _askTypes = { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
    [ObservableProperty] private Language? _currentLanguage;
    [ObservableProperty] private int _disableDependenciesWhenDeleting;
    [ObservableProperty] private int _disableDependenciesWhenDisabling;
    [ObservableProperty] private int _enableDependenciesWhenEnabling;
    [ObservableProperty] private int _enableDependenciesWhenInstalling;
    [ObservableProperty] private string? _path;
    public List<Language> AvailableLanguages => _localizationService.AvailableLanguages;


    public SettingsViewModel()
    {
    }

    public SettingsViewModel(ISettingService settingService, IModManageViewModel modManageViewModel,
        ILocalizationService localizationService)
    {
        _settingService = settingService;
        _modManageViewModel = modManageViewModel;
        _localizationService = localizationService;
        Initialize();
    }

    public void Initialize()
    {
        CurrentLanguage = _settingService.Settings.LanguageCode is null
            ? new Language(CultureInfo.CurrentUICulture)
            : new Language(CultureInfo.GetCultureInfo(_settingService.Settings.LanguageCode));
        Path = _settingService.Settings.MuseDashFolder;
    }

    public void ChangeOptionName()
    {
        AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
    }

    [RelayCommand]
    private void SetLanguage() => _localizationService.SetLanguage(CurrentLanguage!.Name!);

    [RelayCommand]
    private async Task OnChoosePath()
    {
        await _settingService.OnChoosePath();
        _modManageViewModel.Initialize();
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