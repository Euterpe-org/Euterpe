using Semver;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest
{
    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_LowerVersion_ShouldNotFindUpdate()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        var distributionClient = CreateDistributionClientMock(LowerPrereleaseVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerPrereleaseVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    [PrereleaseOnly]
    public async Task CheckForUpdatesAsync_FindPrerelease_CurrentVersion_ShouldNotFindUpdate()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        var distributionClient = CreateDistributionClientMock(AppVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {AppVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenHigherVersionIsSkipped_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        Config.SkipVersion = SemVersion.Parse(HigherPrereleaseVersion);
        var distributionClient = CreateDistributionClientMock(HigherPrereleaseVersion);

        var updateService = CreateUpdateService(distributionClient: distributionClient);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        var distributionClient = CreateDistributionClientMock(HigherPrereleaseVersion);
        var messageBoxServiceMock = IMessageBoxService.Mock();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(distributionClient: distributionClient, messageBoxService: messageBoxServiceMock);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherPrereleaseVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}