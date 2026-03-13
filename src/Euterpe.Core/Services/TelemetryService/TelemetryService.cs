namespace Euterpe.Core;

internal sealed partial class TelemetryService : ITelemetryService
{
    private HttpClient ApiClient => HttpClientFactory.CreateClient(EuterpeApi.HttpClientName);

    public async Task TrackVisitorAsync()
    {
        try
        {
            await PostVisitorTelemetryAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to record visitor statistics");
        }
    }

    public async Task TrackDownloadAsync(string modName, string modAuthor)
    {
        try
        {
            await PostDownloadTelemetryAsync(modName, modAuthor).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to record download statistics");
        }
    }

    #region Injections

    [UsedImplicitly]
    public required IHttpClientFactory HttpClientFactory { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required ILogger<TelemetryService> Logger { get; init; }

    #endregion Injections
}