using Euterpe.Contracts.Telemetry;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

internal interface ITelemetryApiClient
{
    [Post(Telemetry.Session)]
    Task<HttpResponseMessage> PostSessionAsync([Body] SessionEvent payload, CancellationToken cancellationToken = default);
}
