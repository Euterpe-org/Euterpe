namespace Euterpe.Tests;

[Category("ModTemplateStepTests")]
[TestSubject(typeof(ModTemplateStep))]
public sealed class ModTemplateStepTest
{
    [Test]
    public async Task ExecuteAsync_AlreadyInstalled_DoesNotInstall()
    {
        var modTemplateInstaller = IGameModTemplateInstaller.Mock();
        modTemplateInstaller.CheckInstalledAsync().Returns(true);
        var step = new ModTemplateStep { ModTemplateInstaller = modTemplateInstaller };

        await step.ExecuteAsync();

        modTemplateInstaller.InstallAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_NotInstalled_InstallsIt()
    {
        var modTemplateInstaller = IGameModTemplateInstaller.Mock();
        modTemplateInstaller.CheckInstalledAsync().Returns(false);
        var step = new ModTemplateStep { ModTemplateInstaller = modTemplateInstaller };

        await step.ExecuteAsync();

        modTemplateInstaller.InstallAsync().WasCalled(Times.Once);
    }
}
