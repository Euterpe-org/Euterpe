using Euterpe.Contracts.Telemetry;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Abstractions;

public interface ITelemetryApiClient
{
    [Post(Telemetry.Session)]
    Task<HttpResponseMessage> PostSessionAsync([Body] SessionEvent payload, CancellationToken cancellationToken = default);

    [Post(Telemetry.ModDownload)]
    Task<HttpResponseMessage> PostModDownloadAsync([Body] ModDownloadEvent payload, CancellationToken cancellationToken = default);
}