namespace Euterpe.Tests;

[Category("DotNetSdkStepTests")]
[TestSubject(typeof(DotNetSdkStep))]
public sealed class DotNetSdkStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var sdkInstaller = IDotNetSdkInstaller.Mock();
        sdkInstaller.CheckInstalledAsync().Returns(true);
        var step = new DotNetSdkStep { SdkInstaller = sdkInstaller };

        await step.ExecuteAsync();

        sdkInstaller.InstallAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var sdkInstaller = IDotNetSdkInstaller.Mock();
        sdkInstaller.CheckInstalledAsync().Returns(false);
        var step = new DotNetSdkStep { SdkInstaller = sdkInstaller };

        await step.ExecuteAsync();

        sdkInstaller.InstallAsync().WasCalled(Times.Once);
    }
}