using System.Globalization;
using Autofac;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    private readonly IFontManageService _fontManageService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly ISavingService _savingService;
    private readonly IUpdateUIService _updateUIService;
    [ObservableProperty] private string[] _askTypes;
    [ObservableProperty] private int _currentDownloadSource;
    [ObservableProperty] private int _currentFontIndex;
    [ObservableProperty] private int _currentLanguageIndex;
    [ObservableProperty] private int _disableDependenciesWhenDeleting;
    [ObservableProperty] private int _disableDependenciesWhenDisabling;
    [ObservableProperty] private bool _downloadPrerelease;
    [ObservableProperty] private string[] _downloadSources;
    [ObservableProperty] private int _enableDependenciesWhenEnabling;
    [ObservableProperty] private int _enableDependenciesWhenInstalling;
    [ObservableProperty] private string? _path;
    public List<Language> AvailableLanguages => _localizationService.AvailableLanguages;
    public List<string> AvailableFonts => _fontManageService.AvailableFonts;

    public SettingsViewModel(IComponentContext context)
    {
        _fontManageService = context.Resolve<IFontManageService>();
        _localizationService = context.Resolve<ILocalizationService>();
        _logger = context.Resolve<ILogger>();
        _savingService = context.Resolve<ISavingService>();
        _updateUIService = context.Resolve<IUpdateUIService>();
        Initialize();
    }

    public void UpdatePath() => Path = _savingService.Settings.MuseDashFolder;

    private void Initialize()
    {
        if (_savingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_savingService.Settings.LanguageCode);
        AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
        DownloadSources = new[] { XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee };
        CurrentLanguageIndex = AvailableLanguages.FindIndex(x => x.Name == CultureInfo.CurrentUICulture.Name);
        CurrentFontIndex = AvailableFonts.FindIndex(x => x == _savingService.Settings.FontName);
        Path = _savingService.Settings.MuseDashFolder;
        CurrentDownloadSource = (int)_savingService.Settings.DownloadSource;
        EnableDependenciesWhenInstalling = (int)_savingService.Settings.AskEnableDependenciesWhenInstalling;
        EnableDependenciesWhenEnabling = (int)_savingService.Settings.AskEnableDependenciesWhenEnabling;
        DisableDependenciesWhenDeleting = (int)_savingService.Settings.AskDisableDependenciesWhenDeleting;
        DisableDependenciesWhenDisabling = (int)_savingService.Settings.AskDisableDependenciesWhenDisabling;
        DownloadPrerelease = _savingService.Settings.DownloadPrerelease;

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
        await _savingService.OnChoosePath();
        await _updateUIService.InitializeTabs();
    }

    #region OnPropertyChanged

    [UsedImplicitly]
    partial void OnCurrentDownloadSourceChanged(int value)
    {
        if (value != -1)
            _savingService.Settings.DownloadSource = (DownloadSources)value;
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenInstallingChanged(int value)
    {
        if (value != -1)
            _savingService.Settings.AskEnableDependenciesWhenInstalling = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenEnablingChanged(int value)
    {
        if (value != -1)
            _savingService.Settings.AskEnableDependenciesWhenEnabling = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDeletingChanged(int value)
    {
        if (value != -1)
            _savingService.Settings.AskDisableDependenciesWhenDeleting = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDisablingChanged(int value)
    {
        if (value != -1)
            _savingService.Settings.AskDisableDependenciesWhenDisabling = (AskType)value;
    }

    partial void OnDownloadPrereleaseChanged(bool value)
    {
        _savingService.Settings.DownloadPrerelease = value;
    }

    #endregion
}