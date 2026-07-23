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

            using var response = await TelemetryClient.TrackSessionAsync(payload).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to track session telemetry");
        }
    }

    #region Injections

    public required IEuterpeTelemetryClient TelemetryClient { get; init; }
    public required IPlatformInfo PlatformInfo { get; init; }
    public required ILogger<TelemetryService> Logger { get; init; }

    #endregion Injections
}
