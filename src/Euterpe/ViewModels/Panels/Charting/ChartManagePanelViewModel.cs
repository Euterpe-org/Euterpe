namespace Euterpe.ViewModels.Panels.Charting;

public sealed class ChartManagePanelViewModel : ViewModelBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required IChartManageService ChartManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}