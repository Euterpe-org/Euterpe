using Euterpe.Core.Http.Clients;

namespace Euterpe.ViewModels.Windows;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public const string WizardHostId = "WizardDialog";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);

        await SettingService.ValidateAsync().ConfigureAwait(true);
        await LocalService.ReadGameInformationAsync().ConfigureAwait(true);
        LocalService.ReadMelonLoaderVersion();
        BindMuseDashAccountAsync().SafeFireAndForget();

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
            Title = Wizard_Title_Welcome
        };

        await OverlayDialog.ShowStandardAsync<WizardDialog, WizardDialogViewModel>(WizardDialogViewModel, WizardHostId, options).ConfigureAwait(false);
    }

    private async Task BindMuseDashAccountAsync()
    {
        var request = await PlatformService.GetMuseDashUidRequestAsync().ConfigureAwait(false);
        if (request is null)
        {
            Logger.ZLogWarning($"Failed to get MuseDash user ID. Skipping account binding.");
            return;
        }

        try
        {
            await AccountClient.BindVanillaAccountAsync(request).ConfigureAwait(false);
            Logger.ZLogInformation($"Successfully bound MuseDash account.");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to bind MuseDash account.");
        }
    }

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required WizardDialogViewModel WizardDialogViewModel { get; init; }

    [UsedImplicitly]
    public required IEuterpeAccountClient AccountClient { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}