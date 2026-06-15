using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class DependencyAcquireServiceTest
{
    [Test]
    public async Task GetLatestMelonLoaderVersionAsync_Success_ReturnsVersion()
    {
        var client = CreateClientReturning(CreateDependency("MelonLoader", TestMelonLoaderVersion, "any-sha"));
        var sut = CreateService(client);

        var result = await sut.GetLatestMelonLoaderVersionAsync();

        await Assert.That(result).IsEqualTo(TestMelonLoaderVersion);
    }

    [Test]
    public async Task GetLatestMelonLoaderVersionAsync_ClientThrows_Propagates()
    {
        var client = IEuterpeDistributionClient.Mock();
        client.GetLatestDependenciesAsync(true, Any<CancellationToken>())
            .Throws(new HttpRequestException("network down"));
        var sut = CreateService(client);

        var act = async () => await sut.GetLatestMelonLoaderVersionAsync();

        await Assert.That(act).Throws<HttpRequestException>();
    }

    [Test]
    public async Task GetLatestMelonLoaderVersionAsync_CalledTwice_FetchesOnlyOnce()
    {
        var client = IEuterpeDistributionClient.Mock();
        client.GetLatestDependenciesAsync(true, Any<CancellationToken>())
            .Returns([CreateDependency("MelonLoader", TestMelonLoaderVersion, "any-sha")]);
        var sut = CreateService(client);

        await sut.GetLatestMelonLoaderVersionAsync();
        await sut.GetLatestMelonLoaderVersionAsync();

        client.GetLatestDependenciesAsync(true, Any<CancellationToken>()).WasCalled(Times.Once);
    }
}
