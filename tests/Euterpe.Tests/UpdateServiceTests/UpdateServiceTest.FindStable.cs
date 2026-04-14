using Euterpe.Core.Http.Clients;
using Semver;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest
{
    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerVersion_ShouldNotFindUpdate()
    {
        var distributionClient = CreateDistributionClientMock(LowerStableVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerStableVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    [StableReleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_CurrentVersion_ShouldNotFindUpdate()
    {
        var distributionClient = CreateDistributionClientMock(AppVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {AppVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenHigherVersionIsSkipped_ShouldSkipVersion()
    {
        Config.SkipVersion = SemVersion.Parse(HigherStableVersion);
        var distributionClient = CreateDistributionClientMock(HigherStableVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        var distributionClient = CreateDistributionClientMock(HigherStableVersion);
        var messageBoxServiceMock = IMessageBoxService.Mock();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(distributionClient: distributionClient, messageBoxService: messageBoxServiceMock);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherStableVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherStableVersion));
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenNoMatchingReleaseForRuntime_ShouldReturnFalse()
    {
        var distributionClient = IEuterpeDistributionClient.Mock();
        distributionClient.GetAppReleaseAsync(Any<bool>(), Any<bool>(), Any<CancellationToken>())
            .Returns([]);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
    }
}