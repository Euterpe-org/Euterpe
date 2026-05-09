namespace Euterpe.Core.Http.Handlers;

internal sealed class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        await response.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        logger.ZLogWarning($"HTTP {(int)response.StatusCode} {request.Method} {request.RequestUri}: {body}");

        return response;
    }
}