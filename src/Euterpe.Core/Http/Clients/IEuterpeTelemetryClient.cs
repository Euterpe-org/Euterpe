using Euterpe.Contracts.Telemetry;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeTelemetryClient
{
    [Post(Telemetry.Session)]
    Task<HttpResponseMessage> TrackSessionAsync([Body] SessionEvent payload, CancellationToken cancellationToken = default);
}
