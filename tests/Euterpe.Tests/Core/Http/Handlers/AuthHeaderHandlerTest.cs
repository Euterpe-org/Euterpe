using System.Net;
using Euterpe.Core.Http.Handlers;
using Euterpe.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe.Tests.Core.Http.Handlers;

[Category("AuthHeaderHandlerTests")]
[TestSubject(typeof(AuthHeaderHandler))]
public sealed class AuthHeaderHandlerTest
{
    private static IServiceProvider BuildServices(IAuthService authService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authService);
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task SendAsync_AddsBearerTokenHeader()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token-123");
        var inner = new FakeHttpMessageHandler();
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        var request = inner.Requests.Single();
        using var assertions = Assert.Multiple();
        await Assert.That(request.Headers.Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(request.Headers.Authorization?.Parameter).IsEqualTo("token-123");
    }

    [Test]
    public async Task SendAsync_OnUnauthorized_RenewsTokenAndRetries()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("renewed");
        var inner = new FakeHttpMessageHandler((_, n) =>
            new HttpResponseMessage(n is 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK));
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(inner.CallCount).IsEqualTo(2);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Once);
        await Assert.That(inner.AuthorizationParameters[0]).IsEqualTo("expired");
        await Assert.That(inner.AuthorizationParameters[1]).IsEqualTo("renewed");
    }

    [Test]
    public async Task SendAsync_RetryStillUnauthorized_ReturnsSecondResponse()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("expired");
        auth.RenewAccessTokenAsync(Any<string>()).Returns("still-bad");
        var inner = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(inner.CallCount).IsEqualTo(2);
    }

    [Test]
    public async Task SendAsync_NonUnauthorizedFailure_DoesNotRenew()
    {
        var auth = IAuthService.Mock();
        auth.GetAccessTokenAsync().Returns("token");
        var inner = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        using var handler = new AuthHeaderHandler(BuildServices(auth)) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(inner.CallCount).IsEqualTo(1);
        auth.RenewAccessTokenAsync(Any<string>()).WasCalled(Times.Never);
    }
}
