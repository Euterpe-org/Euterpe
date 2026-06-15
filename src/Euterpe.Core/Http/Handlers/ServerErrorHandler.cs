namespace Euterpe.Core.Http.Handlers;

internal sealed class ServerErrorHandler(ServerErrorNotifier notifier) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if ((int)response.StatusCode >= 500)
        {
            notifier.NotifyIfNeeded(request, response);
        }

        return response;
    }
}
