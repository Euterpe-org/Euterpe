using Downloader;

namespace Euterpe.Tests;

[Category("MelonLoaderStepTests")]
[TestSubject(typeof(MelonLoaderStep))]
public sealed class MelonLoaderStepTest
{
    [Test]
    public async Task ExecuteAsync_AcquiresInstallsAndReadsVersion()
    {
        var depService = IDependencyAcquireService.Mock();
        var localService = ILocalService.Mock();
        localService.InstallMelonLoaderAsync().Returns(true);

        var step = new MelonLoaderStep
        {
            DependencyAcquireService = depService,
            LocalService = localService
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
}