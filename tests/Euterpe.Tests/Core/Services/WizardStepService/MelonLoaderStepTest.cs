using Downloader;

namespace Euterpe.Tests;

[Category("MelonLoaderStepTests")]
[TestSubject(typeof(MelonLoaderStep))]
public sealed class MelonLoaderStepTest
{
    [Test]
    public async Task ExecuteAsync_AcquiresInstallsAndReadsVersion_WhenNotInstalled()
    {
        var depService = IDependencyAcquireService.Mock();
        var localService = IGameLocalService.Mock();
        localService.InstallMelonLoaderAsync().Returns(true);

        var step = new MelonLoaderStep
        {
            DependencyAcquireService = depService,
            GameLocalService = localService,
            GameConfig = new MuseDashConfig()
        };

        await step.ExecuteAsync();

        using var _ = Assert.Multiple();
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Once);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Once);
        localService.ReadMelonLoaderVersion().WasCalled(Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_SkipsInstall_WhenAlreadyInstalled()
    {
        var depService = IDependencyAcquireService.Mock();
        var localService = IGameLocalService.Mock();

        var step = new MelonLoaderStep
        {
            DependencyAcquireService = depService,
            GameLocalService = localService,
            GameConfig = new MuseDashConfig { MelonLoaderVersion = "0.6.5" }
        };

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
}