using System.Net.Http.Json;
using static Euterpe.Core.JsonContexts.CamelCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private IAsyncEnumerable<Chart?> GetChartListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching charts from Website {ChartAPIUrl}...");

        return Client.GetFromJsonAsAsyncEnumerable<Chart>(ChartAPIUrl, Default.Chart, cancellationToken);
    }
}