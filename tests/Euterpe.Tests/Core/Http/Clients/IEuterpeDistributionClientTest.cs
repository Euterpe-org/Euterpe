using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeDistributionClientTests")]
[TestSubject(typeof(IEuterpeDistributionClient))]
public sealed class IEuterpeDistributionClientTest
{
    [Test]
    public async Task GetLatestLibsAsync_DefaultLatest_SendsExpectedQuery()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/api/distribution/libs?latest=True").RespondWithJson("[]");
        var api = http.CreateEuterpeClient<IEuterpeDistributionClient>(Distribution.BasePath);

        var libs = await api.GetLatestLibsAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(libs).IsEmpty();
        await Assert.That(http.Requests.Single().RequestUri!.AbsoluteUri)
            .IsEqualTo("https://euterpe-org.com/api/distribution/libs?latest=True");
    }

    [Test]
    public async Task GetLatestDependenciesAsync_NotLatest_SendsExpectedQuery()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/api/distribution/deps?latest=False").RespondWithJson("[]");
        var api = http.CreateEuterpeClient<IEuterpeDistributionClient>(Distribution.BasePath);

        var dependencies = await api.GetLatestDependenciesAsync(false);

        using var assertions = Assert.Multiple();
        await Assert.That(dependencies).IsEmpty();
        await Assert.That(http.Requests.Single().RequestUri!.AbsoluteUri)
            .IsEqualTo("https://euterpe-org.com/api/distribution/deps?latest=False");
    }
}
