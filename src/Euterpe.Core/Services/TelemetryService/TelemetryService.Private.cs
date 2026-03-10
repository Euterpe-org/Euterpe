using System.Net.Http.Json;
using Euterpe.Models.Statistics;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class TelemetryService
{
    private async Task SendRecordVisitorAsync()
    {
        const string url = StatisticsApiHost + RecordVisitorEndpoint;
        var payload = new RecordVisitorRequest
        {
            Country = RegionInfo.CurrentRegion.TwoLetterISORegionName,
            Platform = PlatformService.OsString,
            Arch = PlatformService.ArchitectureString,
            AppVersion = AppVersion
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-request-id", GenerateRequestId());
        request.Content = JsonContent.Create(payload, Default.RecordVisitorRequest);

        using var response = await Client.SendAsync(request).ConfigureAwait(false);
    }

    private async Task SendRecordDownloadAsync(string modName, string modAuthor)
    {
        const string url = StatisticsApiHost + RecordDownloadEndpoint;
        var payload = new RecordDownloadRequest
        {
            Name = modName,
            Author = modAuthor
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-request-id", GenerateRequestId());
        request.Content = JsonContent.Create(payload, Default.RecordDownloadRequest);

        using var response = await Client.SendAsync(request).ConfigureAwait(false);
    }

    private static string GenerateRequestId() => Guid.CreateVersion7().ToString();
}