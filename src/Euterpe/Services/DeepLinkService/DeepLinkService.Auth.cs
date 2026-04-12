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
            await AuthService.LoginAsync().ConfigureAwait(false);
            return;
        }

        try
        {
            await AuthService.CompleteLoginAsync(code).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Auth callback failed, retrying login");
            await AuthService.LoginAsync().ConfigureAwait(false);
        }
    }
}