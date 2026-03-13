namespace Euterpe.Core;

internal sealed partial class TelemetryService : ITelemetryService
{
    private HttpClient ApiClient => HttpClientFactory.CreateClient(EuterpeApi.HttpClientName);

    public async Task TrackVisitorAsync()
    {
        try
        {
            await PostVisitorAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to track visitor telemetry");
        }
    }

    public async Task TrackModDownloadAsync(string modName, string modAuthor)
    {
        try
        {
            await PostModDownloadAsync(modName, modAuthor).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to track mod download telemetry");
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