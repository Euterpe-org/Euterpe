using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Euterpe.Contracts.Telemetry;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;
using static Euterpe.Shared.EuterpeApi;

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

        await PostAsync(Telemetry.VisitorPath, payload, Default.VisitorEvent).ConfigureAwait(false);
    }

    private async Task PostDownloadAsync(string modName, string modAuthor)
    {
        var payload = new DownloadEvent
        {
            ModName = modName,
            ModAuthor = modAuthor
        };

        await PostAsync(Telemetry.DownloadPath, payload, Default.DownloadEvent).ConfigureAwait(false);
    }

    private async Task PostAsync<T>(string url, T payload, JsonTypeInfo<T> jsonTypeInfo)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-request-id", GenerateRequestId());
        request.Content = JsonContent.Create(payload, jsonTypeInfo);

        using var response = await ApiClient.SendAsync(request).ConfigureAwait(false);
    }

    private static string GenerateRequestId() => Guid.CreateVersion7().ToString();
}