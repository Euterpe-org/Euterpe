using Euterpe.Contracts.Charts;
using static Euterpe.Models.Charts.ChartFiles;

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

        // BuildFileVersions scans each chart folder (synchronous I/O); keep it off the calling UI thread.
        var request = await Task.Run(
                () => new CheckChartUpdatesRequest
                {
                    Charts = charts.ToDictionary(chart => chart.FolderName, BuildFileVersions)
                },
                cancellationToken)
            .ConfigureAwait(false);

        var response = await GameDownloadManager.CheckChartUpdatesAsync(request, cancellationToken).ConfigureAwait(false);

        foreach (var (cid, delta) in response.Charts)
        {
            if (delta.Deleted.Contains(ManifestFileName))
            {
                await RunExclusiveAsync(cid, () => RemoveDelistedChartCoreAsync(cid)).ConfigureAwait(false);
                continue;
            }

            var result = await RunExclusiveAsync(cid, () => UpdateChartCoreAsync(cid, delta.Changed, delta.Deleted, cancellationToken)).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    // Report on-disk chart files, not just manifest entries, so the server's reverse diff prunes orphans the manifest no longer lists.
    private Dictionary<string, ChartFileEntry> BuildFileVersions(ChartDto chart)
    {
        var declared = chart.Manifest.Files;
        return FileSystemService.GetFileSizes(chart.FolderPath).Keys
            .Where(fileName => declared.ContainsKey(fileName) || IsChartFile(fileName))
            .ToDictionary(fileName => fileName, fileName => new ChartFileEntry { Version = declared.GetValueOrDefault(fileName)?.Version ?? 0 });
    }
}
