namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private ChartDto[] GetOnlineCharts() =>
        _sourceCache.Items.Where(chart => chart.Source is ChartSource.Online).ToArray();

    private ChartDto? FindOnlineChartByCid(string cid) =>
        _sourceCache.Items.FirstOrDefault(chart => chart.Source is ChartSource.Online && chart.FolderName == cid);
}
