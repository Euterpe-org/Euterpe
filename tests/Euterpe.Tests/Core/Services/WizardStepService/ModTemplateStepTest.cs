namespace Euterpe.Tests;

[Category("ModTemplateStepTests")]
[TestSubject(typeof(ModTemplateStep))]
public sealed class ModTemplateStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckModTemplateInstalledAsync().Returns(true);
        var step = new ModTemplateStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallModTemplateAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var platformService = IPlatformService.Mock();
        platformService.CheckModTemplateInstalledAsync().Returns(false);
        var step = new ModTemplateStep { PlatformService = platformService };

        await step.ExecuteAsync();

        platformService.InstallModTemplateAsync().WasCalled(Times.Once);
    }
}