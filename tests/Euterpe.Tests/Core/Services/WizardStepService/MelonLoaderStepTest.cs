using Downloader;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

[Category("MelonLoaderStepTests")]
[TestSubject(typeof(MelonLoaderStep))]
public sealed class MelonLoaderStepTest
{
    private readonly MockLogger<MelonLoaderStep> _logger = Mock.Logger<MelonLoaderStep>();

    private MelonLoaderStep CreateStep(IDependencyAcquireService depService, IGameLocalService localService, GameConfig gameConfig) =>
        new()
        {
            DependencyAcquireService = depService,
            GameLocalService = localService,
            GameConfig = gameConfig,
            Logger = _logger
        };

    [Test]
    public async Task ExecuteAsync_Installs_WhenNotInstalled()
    {
        var depService = IDependencyAcquireService.Mock();
        var localService = IGameLocalService.Mock();

        var step = CreateStep(depService, localService, new MuseDashConfig());

        await step.ExecuteAsync();

        using var _ = Assert.Multiple();
        depService.GetLatestMelonLoaderVersionAsync(Any<CancellationToken>()).WasCalled(Times.Never);
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Once);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Once);
        localService.ReadMelonLoaderVersion().WasCalled(Times.Exactly(2));
    }

    [Test]
    public async Task ExecuteAsync_SkipsInstall_WhenAlreadyUpToDate()
    {
        var depService = IDependencyAcquireService.Mock();
        depService.GetLatestMelonLoaderVersionAsync(Any<CancellationToken>()).Returns("0.7.0");
        var localService = IGameLocalService.Mock();

        var step = CreateStep(depService, localService, new MuseDashConfig { MelonLoaderVersion = "0.7.0" });

        await step.ExecuteAsync();

        using var _ = Assert.Multiple();
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Never);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Never);
        localService.ReadMelonLoaderVersion().WasCalled(Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_Throws_WhenLatestUnavailable()
    {
        var depService = IDependencyAcquireService.Mock();
        depService.GetLatestMelonLoaderVersionAsync(Any<CancellationToken>()).Throws(new InvalidOperationException("fetch failed"));
        var localService = IGameLocalService.Mock();

        var step = CreateStep(depService, localService, new MuseDashConfig { MelonLoaderVersion = "0.6.5" });

        var act = async () => await step.ExecuteAsync();
        await Assert.That(act).Throws<InvalidOperationException>();

        using var _ = Assert.Multiple();
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Never);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_Upgrades_WhenOutdated()
    {
        var depService = IDependencyAcquireService.Mock();
        depService.GetLatestMelonLoaderVersionAsync(Any<CancellationToken>()).Returns("0.7.0");
        var localService = IGameLocalService.Mock();

        var step = CreateStep(depService, localService, new MuseDashConfig { MelonLoaderVersion = "0.6.5" });

        await step.ExecuteAsync();

        using var _ = Assert.Multiple();
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Once);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Once);
        localService.ReadMelonLoaderVersion().WasCalled(Times.Exactly(2));
    }
}