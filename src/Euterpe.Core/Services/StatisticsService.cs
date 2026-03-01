using System.Net.Http.Json;
using Euterpe.Models.Statistics;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed class StatisticsService : IStatisticsService
{
    public async Task RecordVisitorAsync()
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

    public async Task RecordDownloadAsync(string modName, string modAuthor)
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

    private async Task SendRecordVisitorAsync()
    {
        const string url = StatisticsApiHost + RecordVisitorEndpoint;
        var payload = new RecordVisitorRequest
        {
            Country = RegionInfo.CurrentRegion.TwoLetterISORegionName,
            Platform = PlatformService.OsString.ToLowerInvariant(),
            Arch = GetArchitecture(),
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

    private static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => "unknown"
        };
    }

    private static string GenerateRequestId() => Guid.CreateVersion7().ToString();

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required ILogger<StatisticsService> Logger { get; init; }

    #endregion Injections
}