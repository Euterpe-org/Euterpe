namespace Euterpe.ViewModels.Windows;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public const string WizardHostId = "WizardDialog";

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        await AppSettingService.ValidateSteamAsync().ConfigureAwait(true);

        if (!Config.SetupCompleted)
        {
            await ShowWizardDialogAsync().ConfigureAwait(true);
        }

        NavigationService.Ready.Set();

        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    private async Task ShowWizardDialogAsync()
    {
        Logger.ZLogInformation($"Showing setup wizard dialog");

        var options = new OverlayDialogOptions
        {
            Title = Wizard_Title_Welcome,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        await OverlayDialog.ShowCustomAsync<WizardDialog, WizardDialogViewModel, object>(WizardDialogViewModel, WizardHostId, options).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required WizardDialogViewModel WizardDialogViewModel { get; init; }

    [UsedImplicitly]
    public required IAppSettingService AppSettingService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}