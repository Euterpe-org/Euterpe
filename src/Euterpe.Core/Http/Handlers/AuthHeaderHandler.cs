using System.Net;
using System.Net.Http.Headers;

namespace Euterpe.Core.Http.Handlers;

internal sealed class AuthHeaderHandler(IAuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is not HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var newToken = await authService.RefreshAccessTokenAsync().ConfigureAwait(false);
        if (newToken is null)
        {
            return response;
        }

        response.Dispose();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}