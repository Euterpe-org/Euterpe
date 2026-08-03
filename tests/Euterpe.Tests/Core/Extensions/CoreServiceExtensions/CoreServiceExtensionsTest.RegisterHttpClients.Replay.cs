using System.Net;
using System.Text;
using Euterpe.Contracts.Charts;
using Euterpe.Core.Http.Clients;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using TUnit.Mocks.Http;

namespace Euterpe.Tests.Core.Extensions;

public sealed partial class CoreServiceExtensionsTest
{
    [Test]
    public async Task RegisterHttpClients_JsonPostUnauthorized_ReplaysIdenticalBody()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("renewed");
        var primary = Mock.HttpHandler();
        var updates = primary.OnPost("/api/charts/check-updates");
        updates.Respond(HttpStatusCode.Unauthorized);
        updates.RespondWithJson("""{"charts":{}}""");
        await using var provider = BuildRefitPipelineProvider(nameof(EuterpeApi.Charts), auth, primary);
        var request = new CheckChartUpdatesRequest
        {
            Charts =
            {
                ["chart-id"] = new Dictionary<string, ChartFileEntry>
                {
                    ["map.bms"] = new() { Version = 7 }
                }
            }
        };

        var response = await provider.GetRequiredService<IEuterpeChartClient>().CheckChartUpdatesAsync(request);

        using var assertions = Assert.Multiple();
        await Assert.That(response.Charts).IsEmpty();
        await Assert.That(primary.Requests.Count).IsEqualTo(2);
        await Assert.That(primary.Requests[0].Body).IsEqualTo(primary.Requests[1].Body);
        await Assert.That(primary.Requests[0].Body).Contains("\"version\":7");
        await Assert.That(primary.Requests[0].Headers["Authorization"].Single()).IsEqualTo("Bearer expired");
        await Assert.That(primary.Requests[1].Headers["Authorization"].Single()).IsEqualTo("Bearer renewed");
        auth.RenewAccessTokenAsync("expired").WasCalled(Times.Once);
    }

    [Test]
    public async Task RegisterHttpClients_MultipartUnauthorized_ReplaysIdenticalBodyAndKeepsStreamOpen()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("renewed");
        var primary = Mock.HttpHandler();
        var upload = primary.OnPost("/api/logs/upload");
        upload.Respond(HttpStatusCode.Unauthorized);
        upload.Respond(HttpStatusCode.NoContent);
        await using var provider = BuildRefitPipelineProvider(nameof(EuterpeApi.Logs), auth, primary);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("critical log"));
        var file = new StreamPart(stream, "app.log.gz", "application/gzip");

        using (var response = await provider.GetRequiredService<IEuterpeLogClient>().UploadLogAsync(file, "app"))
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        }

        using var assertions = Assert.Multiple();
        await Assert.That(primary.Requests.Count).IsEqualTo(2);
        await Assert.That(primary.Requests[0].Body).IsEqualTo(primary.Requests[1].Body);
        await Assert.That(primary.Requests[0].Body).Contains("critical log");
        await Assert.That(stream.CanRead).IsTrue();
        await Assert.That(primary.Requests[0].Headers["Authorization"].Single()).IsEqualTo("Bearer expired");
        await Assert.That(primary.Requests[1].Headers["Authorization"].Single()).IsEqualTo("Bearer renewed");
        auth.RenewAccessTokenAsync("expired").WasCalled(Times.Once);
    }
}
