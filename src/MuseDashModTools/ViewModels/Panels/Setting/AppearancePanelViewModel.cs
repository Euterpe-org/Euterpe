namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class AppearancePanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial int CurrentLanguageIndex { get; set; }

    public Language[] AvailableLanguages => LocalizationService.AvailableLanguages;

    [RelayCommand]
    protected override Task InitializeAsync()
    {
        base.InitializeAsync();

        CurrentLanguageIndex = LocalizationService.GetCurrentLanguageIndex();

        Logger.ZLogInformation($"{nameof(AppearancePanelViewModel)} Initialized");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ApplyLanguage() => LocalizationService.SetLanguage(AvailableLanguages[CurrentLanguageIndex].Name);

    #region Injections

    [UsedImplicitly]

    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppearancePanelViewModel> Logger { get; init; }

    #endregion Injections
}