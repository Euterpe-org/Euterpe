using System.Globalization;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class SettingsViewModel : ViewModelBase, ISettingsViewModel
{
    [ObservableProperty] private string[] _askTypes;
    [ObservableProperty] private int _currentDownloadSource;
    [ObservableProperty] private int _currentFontIndex;
    [ObservableProperty] private int _currentLanguageIndex;
    [ObservableProperty] private string? _customDownloadSource;
    [ObservableProperty] private int _disableDependenciesWhenDeleting;
    [ObservableProperty] private int _disableDependenciesWhenDisabling;
    [ObservableProperty] private bool _downloadPrerelease;
    [ObservableProperty] private string[] _downloadSources;
    [ObservableProperty] private int _enableDependenciesWhenEnabling;
    [ObservableProperty] private int _enableDependenciesWhenInstalling;
    [ObservableProperty] private bool _isUsingCustomDownloadSource;
    [ObservableProperty] private string? _path;

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

    public List<Language> AvailableLanguages => LocalizationService.AvailableLanguages;
    public List<string> AvailableFonts => FontManageService.AvailableFonts;

    public void Initialize()
    {
        if (SavingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(SavingService.Settings.LanguageCode);
        AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
        DownloadSources = new[]
            { XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee, XAML_DownloadSource_Custom };
        CurrentLanguageIndex = AvailableLanguages.FindIndex(x => x.Name == CultureInfo.CurrentUICulture.Name);
        CurrentFontIndex = AvailableFonts.FindIndex(x => x == SavingService.Settings.FontName);
        Path = SavingService.Settings.MuseDashFolder;
        CustomDownloadSource = SavingService.Settings.CustomDownloadSource;
        CurrentDownloadSource = (int)SavingService.Settings.DownloadSource;
        EnableDependenciesWhenInstalling = (int)SavingService.Settings.AskEnableDependenciesWhenInstalling;
        EnableDependenciesWhenEnabling = (int)SavingService.Settings.AskEnableDependenciesWhenEnabling;
        DisableDependenciesWhenDeleting = (int)SavingService.Settings.AskDisableDependenciesWhenDeleting;
        DisableDependenciesWhenDisabling = (int)SavingService.Settings.AskDisableDependenciesWhenDisabling;
        DownloadPrerelease = SavingService.Settings.DownloadPrerelease;
        IsUsingCustomDownloadSource = CurrentDownloadSource == 3;

        Logger.Information("Settings Window initialized");
    }

    [RelayCommand]
    private void SetLanguage() => LocalizationService.SetLanguage(AvailableLanguages[CurrentLanguageIndex].Name!);

    [RelayCommand]
    private void SetFont() => FontManageService.SetFont(AvailableFonts[CurrentFontIndex]);

    [RelayCommand]
    private async Task OnChoosePath()
    {
        Logger.Information("Choose path button clicked");
        await SavingService.OnChoosePath();
        UpdatePath();
        await UpdateUIService.InitializeTabsOnChoosePath();
    }

    private void UpdatePath() => Path = SavingService.Settings.MuseDashFolder;

    #region OnPropertyChanged

    [UsedImplicitly]
    partial void OnCurrentDownloadSourceChanged(int value)
    {
        if (value != -1)
            SavingService.Settings.DownloadSource = (DownloadSources)value;

        IsUsingCustomDownloadSource = value == 3;
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenInstallingChanged(int value)
    {
        if (value != -1)
            SavingService.Settings.AskEnableDependenciesWhenInstalling = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnEnableDependenciesWhenEnablingChanged(int value)
    {
        if (value != -1)
            SavingService.Settings.AskEnableDependenciesWhenEnabling = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDeletingChanged(int value)
    {
        if (value != -1)
            SavingService.Settings.AskDisableDependenciesWhenDeleting = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnDisableDependenciesWhenDisablingChanged(int value)
    {
        if (value != -1)
            SavingService.Settings.AskDisableDependenciesWhenDisabling = (AskType)value;
    }

    [UsedImplicitly]
    partial void OnDownloadPrereleaseChanged(bool value) => SavingService.Settings.DownloadPrerelease = value;

    [UsedImplicitly]
    partial void OnCustomDownloadSourceChanged(string? value) => SavingService.Settings.CustomDownloadSource = value;

    #endregion
}