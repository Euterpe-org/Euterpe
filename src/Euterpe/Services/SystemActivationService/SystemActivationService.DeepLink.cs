using System.Web;

namespace Euterpe.Services;

public sealed partial class SystemActivationService
{
    private IChartManageService ChartManageService => GameScope.Value.Resolve<IChartManageService>();
    private IModManageService ModManageService => GameScope.Value.Resolve<IModManageService>();

    private void HandleDeepLink(Uri uri)
    {
        if (ShouldActivateWindow(uri.Query))
        {
            ActivateMainWindow(true);
        }

        var action = uri.Host;
        var path = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

        HandleActionAsync(action, path).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to handle deep link: {uri}"));
    }

    private static bool ShouldActivateWindow(string query) =>
        !bool.TryParse(HttpUtility.ParseQueryString(query)["silent"], out var silent) || !silent;

    private async Task HandleActionAsync(string action, string path)
    {
        switch (action)
        {
            case "mod":
                await NavigationService.NavigateToAsync("/modding/manage").ConfigureAwait(true);
                await ModManageService.InitializeModsAsync().ConfigureAwait(true);
                await HandleModActionAsync(path).ConfigureAwait(false);
                break;

            case "chart":
                await NavigationService.NavigateToAsync("/charting/manage").ConfigureAwait(true);
                await ChartManageService.InitializeChartsAsync().ConfigureAwait(true);
                await HandleChartActionAsync(path).ConfigureAwait(false);
                break;

            case "go":
                await NavigationService.NavigateToAsync($"/{path}").ConfigureAwait(false);
                break;

            default:
                Logger.ZLogWarning($"Unknown deep link action '{action}' with path '{path}'");
                break;
        }
    }
}
