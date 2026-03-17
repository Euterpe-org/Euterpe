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

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || parsed.Scheme != IPlatformService.DeepLinkScheme)
        {
            Logger.ZLogWarning($"Invalid deep link URI: {uri}");
            return;
        }

        ActivateMainWindow(true);

        var action = parsed.Host;
        var path = parsed.AbsolutePath.Trim('/');

        Logger.ZLogInformation($"Deep link action: {action}, path: {path}");

        switch (action)
        {
            case "mod":
                HandleModActionAsync(path).SafeFireAndForget();
                break;

            default:
                Logger.ZLogWarning($"Unknown deep link action: {action}");
                break;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<DeepLinkService> Logger { get; init; }

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}