namespace Euterpe;

[Register]
public sealed partial class AppViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

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

    public required ILogger<AppViewModel> Logger { get; init; }
#if PUBLISH
    public required ITelemetryService TelemetryService { get; init; }
#endif

    #endregion Injections
}
