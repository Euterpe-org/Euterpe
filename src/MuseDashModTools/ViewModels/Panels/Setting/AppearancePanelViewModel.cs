namespace MuseDashModTools.ViewModels.Panels.Setting;

public sealed partial class AppearancePanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyLanguageCommand))]
    public partial Language? CurrentLanguage { get; set; }

    public Language[] AvailableLanguages => LocalizationService.AvailableLanguages;

    [RelayCommand]
    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        CurrentLanguage = LocalizationService.GetCurrentLanguage();

        Logger.ZLogInformation($"{nameof(AppearancePanelViewModel)} Initialized");
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteApplyLanguage))]
    private void ApplyLanguage() => LocalizationService.SetLanguage(CurrentLanguage!.Name);

    private bool CanExecuteApplyLanguage() => CurrentLanguage is not null;

    #region Injections

    [UsedImplicitly]

    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppearancePanelViewModel> Logger { get; init; }

    #endregion Injections
}