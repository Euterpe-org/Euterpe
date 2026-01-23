namespace Euterpe.ViewModels;

public sealed partial class AppViewModel : ViewModelBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        await SettingService.LoadAsync().ConfigureAwait(false);

        StatisticsService.RecordVisitor();

        Logger.ZLogInformation($"{nameof(AppViewModel)} Initialized");
    }

    [RelayCommand]
    private static void Show()
    {
    }

    [RelayCommand]
    private static void Exit() => GetCurrentDesktop().Shutdown();

    #region Injections

    [UsedImplicitly]
    public required ILogger<AppViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    [UsedImplicitly]
    public required IStatisticsService StatisticsService { get; init; }

    #endregion Injections
}