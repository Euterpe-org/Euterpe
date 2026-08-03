using System.Net;
using Euterpe.Core.Http.Clients;
using Refit;
using TUnit.Mocks.Http;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeHealthClientTests")]
[TestSubject(typeof(IEuterpeHealthClient))]
public sealed class IEuterpeHealthClientTest
{
    [Test]
    public async Task CheckAsync_HealthEndpoint_ReturnsRawResponse()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/health").Respond(HttpStatusCode.OK);
        var api = RestService.ForGenerated<IEuterpeHealthClient>(
            http.ThrowOnUnmatched().CreateClient(EuterpeWeb.BaseUrl));

        using var response = await api.CheckAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(http.Requests.Single().RequestUri!.AbsoluteUri)
            .IsEqualTo("https://euterpe-org.com/health");
    }
}
