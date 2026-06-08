using Euterpe.Contracts.Charts;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task CheckAndApplyUpdatesAsync(ChartDto[] charts, CancellationToken cancellationToken)
    {
        if (charts is [])
        {
            return;
        }

        var request = new CheckChartUpdatesRequest
        {
            Charts = charts.ToDictionary(
                chart => chart.FolderName,
                chart => chart.Manifest.Files.ToDictionary(
                    file => file.Key,
                    file => new ChartFileEntry { Version = file.Value.Version }))
        };

        var response = await GameDownloadManager.CheckChartUpdatesAsync(request, cancellationToken).ConfigureAwait(false);

        foreach (var (cid, changedFiles) in response.Updates)
        {
            string[] files = [.. changedFiles.Keys];
            await RunExclusiveAsync(cid, () => UpdateChartCoreAsync(cid, files, cancellationToken)).ConfigureAwait(false);
        }

        foreach (var cid in response.Removed)
        {
            await RunExclusiveAsync(cid.ToString(), () => RemoveDelistedChartCoreAsync(cid.ToString())).ConfigureAwait(false);
        }
    }
}