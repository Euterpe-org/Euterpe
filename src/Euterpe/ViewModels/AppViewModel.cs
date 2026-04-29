namespace Euterpe.ViewModels;

public sealed partial class AppViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        await SettingService.LoadAsync().ConfigureAwait(false);
#if PUBLISH
        TelemetryService.TrackSessionAsync().SafeFireAndForget();
#endif

        Logger.ZLogInformation($"{nameof(AppViewModel)} Initialized");
    }

    [RelayCommand]
    private static void Show() => ActivateMainWindow();

    [RelayCommand]
    private static void Exit() => GetCurrentDesktop().Shutdown();

    #region Injections

    [UsedImplicitly]
    public required ILogger<AppViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

#if PUBLISH
    [UsedImplicitly]
    public required ITelemetryService TelemetryService { get; init; }
#endif

    #endregion Injections
}