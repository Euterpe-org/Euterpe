using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core;

internal sealed partial class TelemetryService
{
    private async Task PostSessionAsync()
    {
        var payload = new SessionEvent(
            RegionInfo.CurrentRegion.TwoLetterISORegionName,
            PlatformService.OsString,
            PlatformService.ArchitectureString,
            AppVersion);

        using var response = await TelemetryApiClient.PostSessionAsync(payload).ConfigureAwait(false);
    }
}