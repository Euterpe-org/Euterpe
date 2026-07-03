using System.Net;
using Euterpe.Contracts.Telemetry;
using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("ITelemetryApiClientTests")]
[TestSubject(typeof(ITelemetryApiClient))]
public sealed class ITelemetryApiClientTest
{
    [Test]
    public async Task TrackSessionAsync_ServerError_ReturnsResponseWithoutThrowing()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/telemetry/app/session").Respond(HttpStatusCode.InternalServerError);
        var api = http.CreateEuterpeClient<ITelemetryApiClient>(Telemetry.BasePath);

        using var response = await api.TrackSessionAsync(new SessionEvent("JP", "windows", "x64", "1.2.3"));

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert.That(http.Requests[0].Body).Contains("\"app_version\":\"1.2.3\"");
    }
}
