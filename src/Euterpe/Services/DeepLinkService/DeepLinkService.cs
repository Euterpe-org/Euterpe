namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    public async Task SetupAsync()
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Process path is null");
        await PlatformService.SetupDeepLinkAsync(processPath).ConfigureAwait(false);
    }

    public void HandleStartupArgs(string[] args)
    {
        if (args is [])
        {
            return;
        }

        HandleUri(args[0]);
    }

    public void HandleUri(string uri)
    {
        Logger.ZLogInformation($"Deep link received: {uri}");

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || parsed.Scheme is not IPlatformService.DeepLinkScheme)
        {
            Logger.ZLogWarning($"Invalid deep link: {uri}");
            return;
        }

        ActivateMainWindow(true);

        var action = parsed.Host;
        var path = parsed.AbsolutePath.TrimStart('/');
        var query = parsed.Query.TrimStart('?');

        HandleActionAsync(action, path, query).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to handle deep link: {uri}"));
    }

    private async Task HandleActionAsync(string action, string path, string query)
    {
        switch (action)
        {
            case "mod":
                await HandleModActionAsync(path).ConfigureAwait(false);
                break;

            case "go":
                NavigationService.NavigateTo($"/{path}");
                break;

            default:
                Logger.ZLogWarning($"Unknown deep link action '{action}' with path '{path}' and query '{query}'");
                break;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required ILogger<DeepLinkService> Logger { get; init; }

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}