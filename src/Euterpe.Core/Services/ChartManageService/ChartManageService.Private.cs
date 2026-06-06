namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task LoadChartsAsync()
    {
        await LoadFromSourceAsync(ChartSource.Offline).ConfigureAwait(false);
        await LoadFromSourceAsync(ChartSource.Online).ConfigureAwait(false);

        Logger.ZLogInformation($"All charts loaded");
    }

    private async Task LoadFromSourceAsync(ChartSource source)
    {
        var charts = (await ChartLocalService.GetChartFolderPaths(source)
                .WhenAllAsync(folder => ChartLocalService.LoadChartFromPathAsync(folder, source)).ConfigureAwait(false))
            .OfType<ChartDto>()
            .ToArray();

        _sourceCache.AddOrUpdate(charts);
        Logger.ZLogInformation($"Loaded {charts.Length} {source} chart(s)");
    }
}