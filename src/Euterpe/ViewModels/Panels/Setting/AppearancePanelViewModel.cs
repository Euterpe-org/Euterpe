namespace Euterpe.ViewModels.Panels.Setting;

public sealed partial class AppearancePanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyLanguageCommand))]
    public partial Language? CurrentLanguage { get; set; }

    public Language[] AvailableLanguages => LocalizationService.AvailableLanguages;

    protected override Task OnInitializeAsync()
    {
        CurrentLanguage = LocalizationService.GetCurrentLanguage();

        Logger.ZLogInformation($"{nameof(AppearancePanelViewModel)} Initialized");
        return base.OnInitializeAsync();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteApplyLanguage))]
    private void ApplyLanguage() => LocalizationService.SetLanguage(CurrentLanguage!.Name);

    private bool CanExecuteApplyLanguage() => CurrentLanguage is not null;

    #region Injections

    [UsedImplicitly]

    public required Config Config { get; init; }

    [UsedImplicitly]
    public required LocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppearancePanelViewModel> Logger { get; init; }

    #endregion Injections
}