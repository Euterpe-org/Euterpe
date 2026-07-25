using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeAccountClientTests")]
[TestSubject(typeof(IEuterpeAccountClient))]
public sealed class IEuterpeAccountClientTest
{
    [Test]
    public async Task GetCurrentUserAsync_GoldenServerJson_MapsExplicitWireNameOverrides()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/api/me")
            .RespondWithJson(
                """{"user":{"uid":7,"role":1,"email":"user@euterpe.test","nickname":"N","avatar_url":"https://euterpe-org.com/a.png","banned":false,"has_github":true,"has_google":true}}""");
        var api = http.CreateEuterpeClient<IEuterpeAccountClient>(Account.BasePath);

        var response = await api.GetCurrentUserAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(response.User.HasGitHub).IsTrue();
        await Assert.That(response.User.HasGoogle).IsTrue();
        await Assert.That(response.User.AvatarUrl).IsEqualTo("https://euterpe-org.com/a.png");
    }
}
