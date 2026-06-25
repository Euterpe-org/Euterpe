using Downloader;

namespace Euterpe.Tests.Core;

[Category("MelonLoaderStepTests")]
[TestSubject(typeof(MelonLoaderStep))]
public sealed class MelonLoaderStepTest
{
    private static MelonLoaderStep CreateStep(IDependencyAcquireService depService, IGameLocalService localService) =>
        new()
        {
            DependencyAcquireService = depService,
            GameLocalService = localService
        };

    [Test]
    public async Task ExecuteAsync_Reinstalls_Unconditionally()
    {
        var depService = IDependencyAcquireService.Mock();
        var localService = IGameLocalService.Mock();

        var step = CreateStep(depService, localService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        using var _ = Assert.Multiple();
        depService.GetLatestMelonLoaderVersionAsync(Any<CancellationToken>()).WasCalled(Times.Never);
        localService.UninstallMelonLoaderAsync().WasCalled(Times.Once);
        depService.AcquireForMelonLoaderAsync(
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Once);
        localService.InstallMelonLoaderAsync().WasCalled(Times.Once);
        localService.ReadMelonLoaderVersion().WasCalled(Times.Once);
    }
}
