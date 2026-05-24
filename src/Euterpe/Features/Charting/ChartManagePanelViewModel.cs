namespace Euterpe.Features.Charting;

public sealed class ChartManagePanelViewModel : ViewModelBase
{
    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    #region Injections

    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}