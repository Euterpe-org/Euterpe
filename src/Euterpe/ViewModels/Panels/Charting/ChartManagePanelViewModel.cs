namespace Euterpe.ViewModels.Panels.Charting;

public sealed class ChartManagePanelViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required IChartManageService ChartManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}