namespace Euterpe.ViewModels;

public sealed partial class AppViewModel : ViewModelBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        await SettingService.LoadAsync().ConfigureAwait(false);
#if PUBLISH
        await TelemetryService.RecordVisitorAsync().ConfigureAwait(false);
#endif

        Logger.ZLogInformation($"{nameof(AppViewModel)} Initialized");
    }

    [RelayCommand]
    private static void Show()
    {
        var mainWindow = GetCurrentMainWindow();
        if (mainWindow.WindowState is WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.ShowInTaskbar = true;
        }

        mainWindow.Activate();
    }

    [RelayCommand]
    private static void Exit() => GetCurrentDesktop().Shutdown();

    #region Injections

    [UsedImplicitly]
    public required ILogger<AppViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    [UsedImplicitly]
    public required ITelemetryService TelemetryService { get; init; }

    #endregion Injections
}