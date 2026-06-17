using Euterpe.Contracts.Charts;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeChartClient
{
    [Post(Charts.CheckUpdates)]
    Task<CheckChartUpdatesResponse> CheckChartUpdatesAsync([Body] CheckChartUpdatesRequest request, CancellationToken cancellationToken = default);
}
