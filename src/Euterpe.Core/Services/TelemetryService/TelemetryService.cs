namespace Euterpe.Core;

internal sealed partial class TelemetryService : ITelemetryService
{
    public async Task TrackVisitorAsync()
    {
        try
        {
            await SendRecordVisitorAsync().ConfigureAwait(false);
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
            await SendRecordDownloadAsync(modName, modAuthor).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to record download statistics");
        }
    }

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required ILogger<TelemetryService> Logger { get; init; }

    #endregion Injections
}