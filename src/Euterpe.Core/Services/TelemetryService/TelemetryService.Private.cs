using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core;

internal sealed partial class TelemetryService
{
    private async Task PostVisitorAsync()
    {
        var payload = new VisitorEvent
        {
            Country = RegionInfo.CurrentRegion.TwoLetterISORegionName,
            Platform = PlatformService.OsString,
            Architecture = PlatformService.ArchitectureString,
            AppVersion = AppVersion
        };

        using var response = await TelemetryApiClient.PostVisitorAsync(payload).ConfigureAwait(false);
    }

    private async Task PostModDownloadAsync(string modName, string modAuthor)
    {
        var payload = new ModDownloadEvent
        {
            ModName = modName,
            ModAuthor = modAuthor
        };

        using var response = await TelemetryApiClient.PostModDownloadAsync(payload).ConfigureAwait(false);
    }
}