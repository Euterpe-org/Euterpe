using System.Net.Http.Json;

namespace MuseDashModTools.Core;

internal sealed partial class ChartManageService
{
    private IAsyncEnumerable<Chart?> GetChartListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching charts from Website {ChartAPIUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Chart>(ChartAPIUrl, Default.Chart, cancellationToken)
            .Catch<Chart?, Exception>(ex =>
            {
                Logger.ZLogError(ex, $"Failed to fetch charts from Website");
                return AsyncEnumerable.Empty<Chart?>();
            });
    }
}