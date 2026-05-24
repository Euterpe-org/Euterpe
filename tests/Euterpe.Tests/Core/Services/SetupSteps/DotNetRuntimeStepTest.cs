namespace Euterpe.Tests;

[Category("DotNetRuntimeStepTests")]
[TestSubject(typeof(DotNetRuntimeStep))]
public sealed class DotNetRuntimeStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var runtimeInstaller = IGameRuntimeInstaller.Mock();
        runtimeInstaller.CheckInstalledAsync().Returns(true);
        var step = new DotNetRuntimeStep { RuntimeInstaller = runtimeInstaller };

        await step.ExecuteAsync();

        runtimeInstaller.InstallAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var runtimeInstaller = IGameRuntimeInstaller.Mock();
        runtimeInstaller.CheckInstalledAsync().Returns(false);
        var step = new DotNetRuntimeStep { RuntimeInstaller = runtimeInstaller };

        await step.ExecuteAsync();

        runtimeInstaller.InstallAsync().WasCalled(Times.Once);
    }
}