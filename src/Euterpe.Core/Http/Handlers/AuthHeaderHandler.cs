using System.Net;
using System.Net.Http.Headers;

namespace Euterpe.Core.Http.Handlers;

internal sealed class AuthHeaderHandler(IServiceProvider services) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authService = services.GetRequiredService<IAuthService>();
        var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is not HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();

        // If we got a 401, the access token is likely expired. Try to get a new one and retry the request once.
        token = await authService.RenewAccessTokenAsync(token).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
