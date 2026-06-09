using Euterpe.Contracts.Charts;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task<List<ChartUpdateResult>> CheckAndApplyUpdatesAsync(ChartDto[] charts, CancellationToken cancellationToken)
    {
        var results = new List<ChartUpdateResult>();
        if (charts is [])
        {
            return results;
        }

        var request = new CheckChartUpdatesRequest
        {
            Charts = charts.ToDictionary(
                chart => chart.FolderName,
                chart => chart.Manifest.Files.ToDictionary(
                    file => file.Key,
                    file => new ChartFileEntry
                    {
                        Version = file.Value.Version
                    }))
        };

        var response = await GameDownloadManager.CheckChartUpdatesAsync(request, cancellationToken).ConfigureAwait(false);

        foreach (var (cid, changedFiles) in response.Updates)
        {
            string[] files = [.. changedFiles.Keys];
            var result = await RunExclusiveAsync(cid, () => UpdateChartCoreAsync(cid, files, cancellationToken)).ConfigureAwait(false);
            results.Add(result);
        }

        foreach (var cid in response.Removed)
        {
            await RunExclusiveAsync(cid.ToString(), () => RemoveDelistedChartCoreAsync(cid.ToString())).ConfigureAwait(false);
        }

        return results;
    }
}
