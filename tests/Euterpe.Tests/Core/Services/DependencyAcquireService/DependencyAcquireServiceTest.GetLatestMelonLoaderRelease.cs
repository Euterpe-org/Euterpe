using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class DependencyAcquireServiceTest
{
    [Test]
    public async Task GetLatestMelonLoaderReleaseAsync_Success_ReturnsVersionAndRuntime()
    {
        var client = CreateClientReturning(CreateDependency(
            "MelonLoader",
            TestMelonLoaderVersion,
            "any-sha",
            dotNetRuntimeVersion: TestDotNetRuntimeVersion));
        var sut = CreateService(client);

        var result = await sut.GetLatestMelonLoaderReleaseAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result.Version).IsEqualTo(TestMelonLoaderVersion);
        await Assert.That(result.DotNetRuntimeVersion).IsEqualTo(TestDotNetRuntimeVersion);
    }

    [Test]
    public async Task GetLatestMelonLoaderReleaseAsync_ClientThrows_Propagates()
    {
        var client = IEuterpeDistributionClient.Mock();
        client.GetLatestDependenciesAsync(true, Any<CancellationToken>())
            .Throws(new HttpRequestException("network down"));
        var sut = CreateService(client);

        var act = async () => await sut.GetLatestMelonLoaderReleaseAsync();

        await Assert.That(act).Throws<HttpRequestException>();
    }

    [Test]
    public async Task GetLatestMelonLoaderReleaseAsync_CalledTwice_FetchesOnlyOnce()
    {
        var client = IEuterpeDistributionClient.Mock();
        client.GetLatestDependenciesAsync(true, Any<CancellationToken>())
            .Returns([CreateDependency(
                "MelonLoader",
                TestMelonLoaderVersion,
                "any-sha",
                dotNetRuntimeVersion: TestDotNetRuntimeVersion)]);
        var sut = CreateService(client);

        await sut.GetLatestMelonLoaderReleaseAsync();
        await sut.GetLatestMelonLoaderReleaseAsync();

        client.GetLatestDependenciesAsync(true, Any<CancellationToken>()).WasCalled(Times.Once);
    }
}
