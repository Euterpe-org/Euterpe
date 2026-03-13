using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core;

internal sealed class TelemetryApiClient(HttpClient client)
{
    private HttpClient Client { get; } = client;

    public Task<HttpResponseMessage> PostVisitorAsync(
        VisitorEvent payload,
        JsonTypeInfo<VisitorEvent> jsonTypeInfo,
        CancellationToken cancellationToken = default) =>
        Client.PostAsJsonAsync(EuterpeApi.Telemetry.Visitor, payload, jsonTypeInfo, cancellationToken);

    public Task<HttpResponseMessage> PostModDownloadAsync(
        ModDownloadEvent payload,
        JsonTypeInfo<ModDownloadEvent> jsonTypeInfo,
        CancellationToken cancellationToken = default) =>
        Client.PostAsJsonAsync(EuterpeApi.Telemetry.ModDownload, payload, jsonTypeInfo, cancellationToken);
}