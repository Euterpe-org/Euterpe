using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

[Category("EssentialModsStepTests")]
[TestSubject(typeof(EssentialModsStep))]
public sealed class EssentialModsStepTest
{
    private readonly MockLogger<EssentialModsStep> _logger = Mock.Logger<EssentialModsStep>();

    private EssentialModsStep CreateStep(IModManageService modManageService) =>
        new()
        {
            ModManageService = modManageService,
            Logger = _logger
        };

    [Test]
    public async Task Kinds_IsEssentialMods()
    {
        var step = CreateStep(IModManageService.Mock());

        await Assert.That(step.Kinds).IsEqualTo(SetupOptionKinds.EssentialMods);
    }

    [Test]
    public async Task ExecuteAsync_InitializesMods()
    {
        var modManageService = IModManageService.Mock();
        var step = CreateStep(modManageService);

        await step.ExecuteAsync();

        modManageService.InitializeModsAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ReportsProgress_WhenProgressProvided()
    {
        var modManageService = IModManageService.Mock();
        var step = CreateStep(modManageService);
        var reports = new List<string>();
        var progress = new Progress<string>(s => reports.Add(s));

        await step.ExecuteAsync(progress);

        await Task.Yield();
        await Assert.That(reports).Contains("Initializing essential mods ...");
    }

    [Test]
    public async Task ExecuteAsync_DoesNotThrow_WhenProgressIsNull()
    {
        var modManageService = IModManageService.Mock();
        var step = CreateStep(modManageService);

        var act = async () => await step.ExecuteAsync();
        await Assert.That(act).ThrowsNothing();
    }
}