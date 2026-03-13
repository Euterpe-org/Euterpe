namespace Euterpe.Core;

internal sealed class TelemetryApiClient(HttpClient client)
{
    private HttpClient Client { get; } = client;

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
        Client.SendAsync(request, cancellationToken);
}