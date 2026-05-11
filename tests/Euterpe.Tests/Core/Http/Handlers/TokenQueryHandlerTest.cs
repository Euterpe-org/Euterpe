using System.Net;
using System.Web;
using Euterpe.Core.Http.Handlers;
using Euterpe.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Tests;

[Category("TokenQueryHandlerTests")]
[TestSubject(typeof(TokenQueryHandler))]
public sealed class TokenQueryHandlerTest
{
    private static IServiceProvider BuildServices(IAuthService authService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authService);
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task SendAsync_AppendsTokenAsQueryParameter()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token-abc");
        var inner = new FakeHttpMessageHandler();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/files/song");

        var request = inner.Requests.Single();
        var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
        await Assert.That(query["t"]).IsEqualTo("token-abc");
    }

    [Test]
    public async Task SendAsync_PreservesExistingQueryParameters()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("tok");
        var inner = new FakeHttpMessageHandler();
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/files?id=42&kind=audio");

        var request = inner.Requests.Single();
        var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
        using var assertions = Assert.Multiple();
        await Assert.That(query["id"]).IsEqualTo("42");
        await Assert.That(query["kind"]).IsEqualTo("audio");
        await Assert.That(query["t"]).IsEqualTo("tok");
    }

    [Test]
    public async Task SendAsync_OnUnauthorized_RenewsTokenAndRetriesWithNewToken()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("fresh");
        var inner = new FakeHttpMessageHandler((_, n) =>
            new HttpResponseMessage(n == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK));
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/files/song");

        var firstQuery = HttpUtility.ParseQueryString(inner.RequestUris[0]!.Query);
        var secondQuery = HttpUtility.ParseQueryString(inner.RequestUris[1]!.Query);
        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(inner.CallCount).IsEqualTo(2);
        await Assert.That(firstQuery["t"]).IsEqualTo("expired");
        await Assert.That(secondQuery["t"]).IsEqualTo("fresh");
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task SendAsync_NonUnauthorizedFailure_DoesNotRenew()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("tok");
        var inner = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        using var handler = new TokenQueryHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/files/missing");

        using var assertions = Assert.Multiple();
        await Assert.That(inner.CallCount).IsEqualTo(1);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Never);
    }
}