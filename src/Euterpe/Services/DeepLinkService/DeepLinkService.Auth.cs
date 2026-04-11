using System.Web;

namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private async Task HandleAuthCallbackAsync(string query)
    {
        var queryParams = HttpUtility.ParseQueryString(query);
        var code = queryParams["code"];

        if (code.IsNullOrEmpty())
        {
            Logger.ZLogWarning($"Auth callback missing code parameter: {query}");
            return;
        }

        await AuthService.CompleteLoginAsync(code).ConfigureAwait(false);
    }
}