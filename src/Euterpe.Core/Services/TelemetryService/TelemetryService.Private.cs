using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Euterpe.Models.Statistics;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class TelemetryService
{
    private async Task PostVisitorTelemetryAsync()
    {
        var payload = new VisitorTelemetryRequest
        {
            Country = RegionInfo.CurrentRegion.TwoLetterISORegionName,
            Platform = PlatformService.OsString,
            Architecture = PlatformService.ArchitectureString,
            AppVersion = AppVersion
        };

        await PostTelemetryAsync(VisitorTelemetryUrl, payload, Default.VisitorTelemetryRequest).ConfigureAwait(false);
    }

    private async Task PostDownloadTelemetryAsync(string modName, string modAuthor)
    {
        var payload = new DownloadTelemetryRequest
        {
            ModName = modName,
            ModAuthor = modAuthor
        };

        await PostTelemetryAsync(DownloadTelemetryUrl, payload, Default.DownloadTelemetryRequest).ConfigureAwait(false);
    }

    private async Task PostTelemetryAsync<T>(string url, T payload, JsonTypeInfo<T> jsonTypeInfo)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-request-id", GenerateRequestId());
        request.Content = JsonContent.Create(payload, jsonTypeInfo);

        using var response = await Client.SendAsync(request).ConfigureAwait(false);
    }

    private static string GenerateRequestId() => Guid.CreateVersion7().ToString();
}