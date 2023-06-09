using System.Collections.ObjectModel;
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
    [ObservableProperty] private string? _language;
    [ObservableProperty] private string? _path;
    public ObservableCollection<AskType> AskTypes { get; set; } = new();

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

    private void Initialize()
    {
        Language = _settingService.Settings.Language;
        Path = _settingService.Settings.MuseDashFolder;
        AskTypes.Add(_settingService.Settings.AskDisableDependenciesWhenDeleting);
        AskTypes.Add(_settingService.Settings.AskDisableDependenciesWhenDisabling);
        AskTypes.Add(_settingService.Settings.AskEnableDependenciesWhenEnabling);
        AskTypes.Add(_settingService.Settings.AskEnableDependenciesWhenInstalling);
        AskTypes.Add(_settingService.Settings.AskInstallMuseDashModTools);
    }

    [RelayCommand]
    private void SetLanguage() => _localizationService.SetLanguage("en-US");

    [RelayCommand]
    private async Task OnChoosePath()
    {
        await _settingService.OnChoosePath();
        _modManageViewModel.Initialize();
    }
}