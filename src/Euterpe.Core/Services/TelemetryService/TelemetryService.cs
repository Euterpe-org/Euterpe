using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core;

internal sealed class TelemetryService : ITelemetryService
{
    public async Task TrackSessionAsync()
    {
        try
        {
            var payload = new SessionEvent(
                RegionInfo.CurrentRegion.TwoLetterISORegionName,
                PlatformInfo.OsString,
                PlatformInfo.ArchitectureString,
                AppVersion);

            using var response = await TelemetryApiClient.TrackSessionAsync(payload).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to track session telemetry");
        }
    }

    #region Injections

    [UsedImplicitly]
    public required ITelemetryApiClient TelemetryApiClient { get; init; }

    [UsedImplicitly]
    public required IPlatformInfo PlatformInfo { get; init; }

    [UsedImplicitly]
    public required ILogger<TelemetryService> Logger { get; init; }

    #endregion Injections
}