namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private ChartDto[] GetOnlineCharts() =>
        _sourceCache.Items.Where(chart => chart.Source is ChartSource.Online).ToArray();

    private ChartDto[] GetOfflineCharts() =>
        _sourceCache.Items.Where(chart => chart.Source is ChartSource.Offline).ToArray();
}
