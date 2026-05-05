namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private IAuthService AuthService => LazyAuthService.Value;
    private IModManageService ModManageService => LazyModManageService.Value;

    public async Task SetupAsync()
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Process path is null");
        await DeepLinkSetup.SetupDeepLinkAsync(processPath).ConfigureAwait(false);
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

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || parsed.Scheme is not IDeepLinkSetup.DeepLinkScheme)
        {
            Logger.ZLogWarning($"Invalid deep link: {uri}");
            return;
        }

        ActivateMainWindow(true);

        var action = parsed.Host;
        var path = Uri.UnescapeDataString(parsed.AbsolutePath.TrimStart('/'));
        var query = Uri.UnescapeDataString(parsed.Query.TrimStart('?'));

        HandleActionAsync(action, path, query).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to handle deep link: {uri}"));
    }

    private async Task HandleActionAsync(string action, string path, string query)
    {
        switch (action)
        {
            case "mod":
                await NavigationService.Ready.WaitAsync().ConfigureAwait(true);
                await ModManageService.InitializeModsAsync().ConfigureAwait(true);
                await HandleModActionAsync(path).ConfigureAwait(false);
                break;

            case "auth":
                await HandleAuthCallbackAsync(query).ConfigureAwait(false);
                break;

            case "go":
                await NavigationService.NavigateToAsync($"/{path}").ConfigureAwait(false);
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
    public required IDeepLinkSetup DeepLinkSetup { get; init; }

    [UsedImplicitly]
    public required Lazy<IAuthService> LazyAuthService { get; init; }

    [UsedImplicitly]
    public required Lazy<IModManageService> LazyModManageService { get; init; }

    #endregion Injections
}