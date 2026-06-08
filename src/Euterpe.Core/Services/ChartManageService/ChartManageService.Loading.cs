namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task LoadFromSourceAsync(ChartSource source)
    {
        var charts = (await ChartLocalService.GetChartFolderPaths(source)
                .WhenAllAsync(folder => ChartLocalService.LoadChartFromPathAsync(folder, source)).ConfigureAwait(false))
            .OfType<ChartDto>()
            .ToArray();

        _sourceCache.AddOrUpdate(charts);
        Logger.ZLogInformation($"Loaded {charts.Length} {source.ToString()} chart(s)");
    }
}