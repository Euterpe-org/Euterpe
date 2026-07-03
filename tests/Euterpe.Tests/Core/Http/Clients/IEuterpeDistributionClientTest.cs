using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeDistributionClientTests")]
[TestSubject(typeof(IEuterpeDistributionClient))]
public sealed class IEuterpeDistributionClientTest
{
    [Test]
    [Arguments(true, false, "?latest=True&prerelease=False")]
    [Arguments(false, true, "?latest=False&prerelease=True")]
    public async Task GetAppReleaseAsync_BoolArguments_SendsCapitalizedParameterNamedQuery(bool latest, bool prerelease, string expectedQuery)
    {
        using var http = Mock.HttpHandler();
        http.OnRequest(r => r.Method(HttpMethod.Get).PathStartsWith("/api/distribution/app-releases")).RespondWithJson("[]");
        var api = http.CreateEuterpeClient<IEuterpeDistributionClient>(Distribution.BasePath);

        await api.GetAppReleaseAsync(latest, prerelease);

        await Assert.That(http.Requests[0].RequestUri!.Query).IsEqualTo(expectedQuery);
    }
}
