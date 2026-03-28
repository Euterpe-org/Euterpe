namespace Euterpe.Core;

internal sealed partial class TelemetryService : ITelemetryService
{
    public async Task TrackSessionAsync()
    {
        try
        {
            await PostSessionAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to track visitor telemetry");
        }
    }

    #region Injections

    [UsedImplicitly]
    public required ITelemetryApiClient TelemetryApiClient { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required ILogger<TelemetryService> Logger { get; init; }

    #endregion Injections
}