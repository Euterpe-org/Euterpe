using Euterpe.Contracts.Telemetry;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface ITelemetryApiClient
{
    [Post(Telemetry.Session)]
    Task<HttpResponseMessage> TrackSessionAsync([Body] SessionEvent payload, CancellationToken cancellationToken = default);
}
