using Euterpe.Core.Http.Clients;
using Semver;
using TUnit.Mocks.Logging;
using Ursa.Controls;

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

        using var _ = Assert.Multiple();

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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {version}")
            .ContainingMessage("No new version available");
    }

    [Test]
    [Arguments(UpdateChannel.Stable, HigherStableVersion)]
    [Arguments(UpdateChannel.Prerelease, HigherPrereleaseVersion)]
    public async Task CheckForUpdatesAsync_WhenHigherVersionIsSkipped_ShouldSkipVersion(UpdateChannel channel, string version)
    {
        Config.UpdateChannel = channel;
        Config.SkipVersion = SemVersion.Parse(version);
        var distributionClient = CreateDistributionClientMock(version);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
    }

    [Test]
    [Arguments(UpdateChannel.Stable, HigherStableVersion)]
    [Arguments(UpdateChannel.Prerelease, HigherPrereleaseVersion)]
    public async Task CheckForUpdatesAsync_WhenMessageBoxResultNo_ShouldSkipVersion(UpdateChannel channel, string version)
    {
        Config.UpdateChannel = channel;
        var distributionClient = CreateDistributionClientMock(version);
        var messageBoxServiceMock = IMessageBoxService.Mock();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(distributionClient: distributionClient, messageBoxService: messageBoxServiceMock);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {version}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(version));
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
