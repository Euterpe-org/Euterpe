using Euterpe.Contracts.Telemetry;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Abstractions;

public interface ITelemetryApiClient
{
    [Post(Telemetry.Visitor)]
    Task<HttpResponseMessage> PostVisitorAsync(VisitorEvent payload, CancellationToken cancellationToken = default);

    [Post(Telemetry.ModDownload)]
    Task<HttpResponseMessage> PostModDownloadAsync(ModDownloadEvent payload, CancellationToken cancellationToken = default);
}