using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    private readonly ILocalizationService _localizationService;
    private readonly IModManageViewModel _modManageViewModel;
    private readonly ISettingService _settingService;
    [ObservableProperty] private Language _currentLanguage;
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
        CurrentLanguage.Name = _settingService.Settings.LanguageCode;
        CurrentLanguage = AvailableLanguages.Find(x => x.Name == CurrentLanguage.Name)!;
        Path = _settingService.Settings.MuseDashFolder;
    }

    [RelayCommand]
    private void SetLanguage() => _localizationService.SetLanguage(CurrentLanguage.Name!);

    [RelayCommand]
    private async Task OnChoosePath()
    {
        await _settingService.OnChoosePath();
        _modManageViewModel.Initialize();
    }
}