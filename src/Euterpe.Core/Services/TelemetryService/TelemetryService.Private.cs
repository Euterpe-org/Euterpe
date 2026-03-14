using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core;

internal sealed partial class TelemetryService
{
    private async Task PostVisitorAsync()
    {
        var payload = new VisitorEvent(
            RegionInfo.CurrentRegion.TwoLetterISORegionName,
            PlatformService.OsString,
            PlatformService.ArchitectureString,
            AppVersion);

        using var response = await TelemetryApiClient.PostVisitorAsync(payload).ConfigureAwait(false);
    }

    private async Task PostModDownloadAsync(string modName, string modAuthor)
    {
        var payload = new ModDownloadEvent(modName, modAuthor);

        using var response = await TelemetryApiClient.PostModDownloadAsync(payload).ConfigureAwait(false);
    }
}