namespace Euterpe.Tests.Core;

[Category("DotNetRuntimeStepTests")]
[TestSubject(typeof(DotNetRuntimeStep))]
public sealed class DotNetRuntimeStepTest
{
    private const string RuntimeVersion = "8.0";

    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var dependencyAcquireService = CreateDependencyAcquireService();
        var runtimeInstaller = IGameRuntimeInstaller.Mock();
        runtimeInstaller.CheckInstalledAsync(RuntimeVersion).Returns(true);
        var step = new DotNetRuntimeStep
        {
            DependencyAcquireService = dependencyAcquireService,
            RuntimeInstaller = runtimeInstaller
        };

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        runtimeInstaller.InstallAsync(Any<string>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var dependencyAcquireService = CreateDependencyAcquireService();
        var runtimeInstaller = IGameRuntimeInstaller.Mock();
        runtimeInstaller.CheckInstalledAsync(RuntimeVersion).Returns(false);
        var step = new DotNetRuntimeStep
        {
            DependencyAcquireService = dependencyAcquireService,
            RuntimeInstaller = runtimeInstaller
        };

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        runtimeInstaller.InstallAsync(RuntimeVersion).WasCalled(Times.Once);
    }

    private static IDependencyAcquireService CreateDependencyAcquireService()
    {
        var service = IDependencyAcquireService.Mock();
        service.GetLatestMelonLoaderReleaseAsync(Any<CancellationToken>())
            .Returns(new MelonLoaderRelease("0.7.3", RuntimeVersion));
        return service;
    }
}
