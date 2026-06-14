using System.Web;

namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private IChartManageService ChartManageService => GameScope.Value.Resolve<IChartManageService>();
    private IModManageService ModManageService => GameScope.Value.Resolve<IModManageService>();

    public async Task SetupAsync()
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Process path is null");
        await AssociationSetup.RegisterAsync(processPath).ConfigureAwait(false);
    }

    public void HandleStartupArgs(string[] args)
    {
        if (args is [])
        {
            return;
        }

        HandleActivation(args[0]);
    }

    public void HandleActivation(string argument)
    {
        Logger.ZLogInformation($"Activation received: {argument}");

        if (GetEpkPath(argument) is { } epkFilePath)
        {
            ActivateMainWindow(true);
            HandleEpkFileAsync(epkFilePath).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to open EPK file: {epkFilePath}"));
            return;
        }

        if (!Uri.TryCreate(argument, UriKind.Absolute, out var parsed) || parsed.Scheme is not ISystemAssociationSetup.DeepLinkScheme)
        {
            Logger.ZLogWarning($"Unhandled activation: {argument}");
            return;
        }

        var action = parsed.Host;
        var path = Uri.UnescapeDataString(parsed.AbsolutePath.TrimStart('/'));

        if (ShouldActivateWindow(parsed.Query))
        {
            ActivateMainWindow(true);
        }

        HandleActionAsync(action, path).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to handle deep link: {argument}"));
    }

    private static bool ShouldActivateWindow(string query) =>
        !bool.TryParse(HttpUtility.ParseQueryString(query)["silent"], out var silent) || !silent;

    private static string? GetEpkPath(string argument)
    {
        if (Uri.TryCreate(argument, UriKind.Absolute, out var uri))
        {
            return uri.IsFile && uri.LocalPath.EndsWith(ChartFiles.ManifestExtension, StringComparison.OrdinalIgnoreCase)
                ? uri.LocalPath
                : null;
        }

        return argument.EndsWith(ChartFiles.ManifestExtension, StringComparison.OrdinalIgnoreCase) ? argument : null;
    }

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

    #region Injections

    public required NavigationService NavigationService { get; init; }
    public required ILogger<DeepLinkService> Logger { get; init; }
    public required ISystemAssociationSetup AssociationSetup { get; init; }
    public required BehaviorSubject<ILifetimeScope> GameScope { get; init; }

    #endregion Injections
}
