namespace Euterpe.Tests;

[Category("DotNetSdkStepTests")]
[TestSubject(typeof(DotNetSdkStep))]
public sealed class DotNetSdkStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckDotNetSdkInstalledAsync().Returns(true);
        var step = new DotNetSdkStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallDotNetSdkAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckDotNetSdkInstalledAsync().Returns(false);
        var step = new DotNetSdkStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallDotNetSdkAsync().WasCalled(Times.Once);
    }
}