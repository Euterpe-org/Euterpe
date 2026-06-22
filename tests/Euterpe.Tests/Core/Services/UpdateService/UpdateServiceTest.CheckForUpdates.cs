using Euterpe.Core.Http.Clients;
using Semver;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

public sealed partial class UpdateServiceTest
{
    [Test]
    [Arguments(UpdateChannel.Stable, LowerStableVersion)]
    [Arguments(UpdateChannel.Prerelease, LowerPrereleaseVersion)]
    public async Task CheckForUpdatesAsync_LowerVersion_ShouldNotFindUpdate(UpdateChannel channel, string version)
    {
        Config.UpdateChannel = channel;
        var distributionClient = CreateDistributionClientMock(version);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {version}")
            .ContainingMessage("No new version available");
    }

    [Test]
    [Arguments(UpdateChannel.Stable, CurrentStableVersion)]
    [Arguments(UpdateChannel.Prerelease, CurrentPrereleaseVersion)]
    public async Task CheckForUpdatesAsync_RemoteEqualsCurrent_ShouldNotFindUpdate(UpdateChannel channel, string version)
    {
        Config.UpdateChannel = channel;
        var distributionClient = CreateDistributionClientMock(version);

        var updateService = CreateUpdateService(currentVersion: SemVersion.Parse(version), distributionClient: distributionClient);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {version}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_WhenNoMatchingReleaseForRuntime_ShouldReturnFalse()
    {
        var distributionClient = IEuterpeDistributionClient.Mock();
        distributionClient.GetAppReleaseAsync(Any<bool>(), Any<bool>(), Any<CancellationToken>())
            .Returns([]);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
    }
}
