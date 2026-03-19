namespace Euterpe.Core.Http.Handlers;

internal sealed class XRequestIdHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Request-Id", Guid.CreateVersion7().ToString());
        return base.SendAsync(request, cancellationToken);
    }
}