using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("UninstallConflictsStepTests")]
[TestSubject(typeof(UninstallConflictsStep))]
public sealed class UninstallConflictsStepTest
{
    private readonly MockLogger<UninstallConflictsStep> _logger = Mock.Logger<UninstallConflictsStep>();

    private UninstallConflictsStep CreateStep(IModManageService modManageService) =>
        new()
        {
            ModManageService = modManageService,
            Logger = _logger
        };

    [Test]
    public async Task ExecuteAsync_AppModNotFound_DoesNotUninstallAnything()
    {
        var modManageService = IModManageService.Mock();
        modManageService.FindModByName(Any<string>()).Returns((ModDto?)null);

        var step = CreateStep(modManageService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        modManageService.UninstallModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_AppModWithoutIncompatibleMods_DoesNotUninstall()
    {
        var appMod = new ModDto { Name = AppName, IncompatibleMods = [] };
        var modManageService = IModManageService.Mock();
        modManageService.FindModByName(AppName).Returns(appMod);

        var step = CreateStep(modManageService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        modManageService.UninstallModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_IncompatibleModInstalled_UninstallsIt()
    {
        var appMod = new ModDto { Name = AppName, IncompatibleMods = ["EvilMod"] };
        var evilMod = new ModDto { Name = "EvilMod", FileName = "EvilMod.dll" };
        evilMod.AddLocalInfo();
        var modManageService = IModManageService.Mock();
        modManageService.FindModByName(AppName).Returns(appMod);
        modManageService.FindModByName("EvilMod").Returns(evilMod);

        var step = CreateStep(modManageService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        modManageService.UninstallModAsync(evilMod).WasCalled(Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_IncompatibleModNotInstalled_SkipsIt()
    {
        var appMod = new ModDto { Name = AppName, IncompatibleMods = ["MissingMod"] };
        var modManageService = IModManageService.Mock();
        modManageService.FindModByName(AppName).Returns(appMod);
        modManageService.FindModByName("MissingMod").Returns((ModDto?)null);

        var step = CreateStep(modManageService);

        await step.ExecuteAsync(new Progress<string>(_ => { }));

        modManageService.UninstallModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }
}
