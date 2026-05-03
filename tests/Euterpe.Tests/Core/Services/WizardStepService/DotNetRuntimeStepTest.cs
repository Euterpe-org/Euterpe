namespace Euterpe.Tests;

[Category("DotNetRuntimeStepTests")]
[TestSubject(typeof(DotNetRuntimeStep))]
public sealed class DotNetRuntimeStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckDotNetRuntimeInstalledAsync().Returns(true);
        var step = new DotNetRuntimeStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallDotNetRuntimeAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckDotNetRuntimeInstalledAsync().Returns(false);
        var step = new DotNetRuntimeStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallDotNetRuntimeAsync().WasCalled(Times.Once);
    }
}