namespace Euterpe.ViewModels.Windows;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public const string WizardHostId = "WizardDialog";

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        await AppSettingService.ValidateSteamAsync().ConfigureAwait(true);

        NavigationService.Ready.Set();

        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required IAppSettingService AppSettingService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    #endregion Injections
}