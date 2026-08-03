using System.Net;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeAuthClientTests")]
[TestSubject(typeof(IEuterpeAuthClient))]
public sealed class IEuterpeAuthClientTest
{
    private const string GoldenTokenJson =
        """{"access_token":"at-1","refresh_token":"rt-1","me":{"uid":7,"role":0,"email":"user@euterpe.test","nickname":"N","avatar_url":null,"banned":false,"has_github":true,"has_google":false}}""";

    [Test]
    public async Task ExchangeAppTokenAsync_GoldenServerJson_DeserializesTokenAndUser()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/auth/app/token").RespondWithJson(GoldenTokenJson);
        var api = http.CreateEuterpeClient<IEuterpeAuthClient>(Auth.BasePath);

        var response = await api.ExchangeAppTokenAsync(new AppTokenRequest("cid", "code", "verifier", "http://127.0.0.1/cb"));

        using var assertions = Assert.Multiple();
        await Assert.That(response.AccessToken).IsEqualTo("at-1");
        await Assert.That(response.RefreshToken).IsEqualTo("rt-1");
        await Assert.That(response.Me.HasGitHub).IsTrue();
    }

    [Test]
    public async Task ExchangeAppTokenAsync_RequestBody_UsesSnakeCaseWireNames()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/auth/app/token").RespondWithJson(GoldenTokenJson);
        var api = http.CreateEuterpeClient<IEuterpeAuthClient>(Auth.BasePath);

        await api.ExchangeAppTokenAsync(new AppTokenRequest("cid", "code", "verifier", "http://127.0.0.1/cb"));

        var body = http.Requests[0].Body;
        using var assertions = Assert.Multiple();
        await Assert.That(body).Contains("\"client_id\":\"cid\"");
        await Assert.That(body).Contains("\"code_verifier\":\"verifier\"");
        await Assert.That(body).Contains("\"redirect_uri\"");
    }

    [Test]
    public async Task RefreshTokenAsync_RequestBodyAndResponse_UseExpectedWireContract()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/auth/refresh")
            .RespondWithJson("""{"access_token":"new-at","refresh_token":"new-rt"}""");
        var api = http.CreateEuterpeClient<IEuterpeAuthClient>(Auth.BasePath);

        var response = await api.RefreshTokenAsync(new RefreshRequest("old-rt"));

        var request = http.Requests.Single();
        using var assertions = Assert.Multiple();
        await Assert.That(response.AccessToken).IsEqualTo("new-at");
        await Assert.That(response.RefreshToken).IsEqualTo("new-rt");
        await Assert.That(request.RequestUri!.AbsoluteUri).IsEqualTo("https://euterpe-org.com/api/auth/refresh");
        await Assert.That(request.Body).IsEqualTo("""{"refresh_token":"old-rt"}""");
    }

    [Test]
    public async Task LogoutAsync_RequestBody_UsesExpectedWireContract()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/auth/logout").Respond(HttpStatusCode.NoContent);
        var api = http.CreateEuterpeClient<IEuterpeAuthClient>(Auth.BasePath);

        await api.LogoutAsync(new LogoutRequest("refresh-token"));

        var request = http.Requests.Single();
        using var assertions = Assert.Multiple();
        await Assert.That(request.RequestUri!.AbsoluteUri).IsEqualTo("https://euterpe-org.com/api/auth/logout");
        await Assert.That(request.Body).IsEqualTo("""{"refresh_token":"refresh-token"}""");
    }
}
