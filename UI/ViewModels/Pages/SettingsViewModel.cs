using System.Globalization;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Pages;

public sealed partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    [ObservableProperty] private string[] _askTypes;
    [ObservableProperty] private int _currentDownloadSource;
    [ObservableProperty] private int _currentFontIndex, _currentLanguageIndex;
    [ObservableProperty] private string? _customDownloadSource;

    [ObservableProperty] private int _disableDependenciesWhenDeleting,
        _disableDependenciesWhenDisabling,
        _enableDependenciesWhenEnabling,
        _enableDependenciesWhenInstalling;

    [ObservableProperty] private bool _downloadPrerelease;
    [ObservableProperty] private string[] _downloadSources;
    [ObservableProperty] private bool _isUsingCustomDownloadSource;
    [ObservableProperty] private string? _path;

    public List<Language> AvailableLanguages => LocalizationService.AvailableLanguages;
    public List<string> AvailableFonts => FontManageService.AvailableFonts;

    public void Initialize()
    {
        if (Settings.LanguageCode is not null)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(Settings.LanguageCode);
        }

        AskTypes = [XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No];
        DownloadSources = [XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee, XAML_DownloadSource_Custom];
        CurrentLanguageIndex = AvailableLanguages.FindIndex(x => x.Name == CultureInfo.CurrentUICulture.Name);
        CurrentFontIndex = AvailableFonts.FindIndex(x => x == Settings.FontName);
        Path = Settings.MuseDashFolder;
        CustomDownloadSource = Settings.CustomDownloadSource;
        CurrentDownloadSource = (int)Settings.DownloadSource;
        EnableDependenciesWhenInstalling = (int)Settings.AskEnableDependencyWhenInstall;
        EnableDependenciesWhenEnabling = (int)Settings.AskEnableDependencyWhenEnable;
        DisableDependenciesWhenDeleting = (int)Settings.AskDisableDependencyWhenDelete;
        DisableDependenciesWhenDisabling = (int)Settings.AskDisableDependencyWhenDisable;
        DownloadPrerelease = Settings.DownloadPrerelease;
        IsUsingCustomDownloadSource = CurrentDownloadSource == 3;

        Logger.Information("Settings Window initialized");
    }

    [RelayCommand]
    private void SetLanguage() => LocalizationService.SetLanguage(AvailableLanguages[CurrentLanguageIndex].Name!);

    [RelayCommand]
    private void SetFont() => FontManageService.SetFont(AvailableFonts[CurrentFontIndex]);

    [RelayCommand]
    private async Task OnChooseGamePathAsync()
    {
        Logger.Information("Choose path button clicked");
        if (!await SavingService.OnChooseGamePathAsync())
        {
            return;
        }

        UpdatePath();
        await UpdateUIService.InitializeTabsOnChoosePathAsync();
    }

    private void UpdatePath() => Path = Settings.MuseDashFolder;

    #region Services

    [UsedImplicitly]
    public IFontManageService FontManageService { get; init; }

    [UsedImplicitly]
    public ILocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    [UsedImplicitly]
    public IUpdateUIService UpdateUIService { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    #endregion

    #region OnPropertyChanged

    [UsedImplicitly]
    partial void OnCurrentDownloadSourceChanged(int value)
    {
        if (value != -1)
        {
            Settings.DownloadSource = (DownloadSources)value;
        }

        IsUsingCustomDownloadSource = value == 3;
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenInstallingChanged(int value)
    {
        if (value != -1)
        {
            Settings.AskEnableDependencyWhenInstall = (AskType)value;
        }
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenEnablingChanged(int value)
    {
        if (value != -1)
        {
            Settings.AskEnableDependencyWhenEnable = (AskType)value;
        }
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDeletingChanged(int value)
    {
        if (value != -1)
        {
            Settings.AskDisableDependencyWhenDelete = (AskType)value;
        }
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDisablingChanged(int value)
    {
        if (value != -1)
        {
            Settings.AskDisableDependencyWhenDisable = (AskType)value;
        }
    }

    [UsedImplicitly]
    partial void OnDownloadPrereleaseChanged(bool value) => Settings.DownloadPrerelease = value;

    [UsedImplicitly]
    partial void OnCustomDownloadSourceChanged(string? value) => Settings.CustomDownloadSource = value;

    #endregion
}