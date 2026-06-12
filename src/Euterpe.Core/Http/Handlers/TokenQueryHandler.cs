using System.Net;
using System.Web;

namespace Euterpe.Core.Http.Handlers;

internal sealed class TokenQueryHandler(IServiceProvider services) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authService = services.GetRequiredService<IAuthService>();
        var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
        request.RequestUri = AppendToken(request.RequestUri!, token);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is not HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();

        token = await authService.RenewAccessTokenAsync(token).ConfigureAwait(false);
        request.RequestUri = AppendToken(request.RequestUri!, token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static Uri AppendToken(Uri uri, string token)
    {
        var builder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Set("t", token);
        builder.Query = query.ToString();
        return builder.Uri;
    }
}
