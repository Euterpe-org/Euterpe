using Euterpe.Models.Progress;

namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
    [Test]
    public async Task ImportAsync_ChartsAndMods_DelegatesAndReturnsResults()
    {
        var chartServiceMock = IChartManageService.Mock();
        chartServiceMock.DownloadChartsAsync(Any<IReadOnlyList<string>>(), Any<IProgress<BatchProgress>?>(), Any<CancellationToken>())
            .Returns([new BulkItemResult("13", BulkItemOutcome.Added)]);
        var modServiceMock = IModManageService.Mock();
        modServiceMock.InstallModsAsync(Any<IReadOnlyList<ModInstallRequest>>(), Any<IProgress<BatchProgress>?>(), Any<CancellationToken>())
            .Returns([new BulkItemResult("ModA", BulkItemOutcome.Skipped)]);
        var service = CreateService(chartServiceMock, modServiceMock);
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash,
            ChartIds = [13],
            Mods = [new GameShareMod { Name = "ModA", IsDisabled = false }]
        };

        var result = await service.ImportAsync(package);

        using var _ = Assert.Multiple();
        await Assert.That(result.ChartResults.Single().Outcome).IsEqualTo(BulkItemOutcome.Added);
        await Assert.That(result.ModResults.Single().Outcome).IsEqualTo(BulkItemOutcome.Skipped);
        chartServiceMock.InitializeChartsAsync().WasCalled(Times.Once);
        modServiceMock.InitializeModsAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task ImportAsync_ChartsOnly_DoesNotInitializeMods()
    {
        var chartServiceMock = IChartManageService.Mock();
        chartServiceMock.DownloadChartsAsync(Any<IReadOnlyList<string>>(), Any<IProgress<BatchProgress>?>(), Any<CancellationToken>())
            .Returns([]);
        var modServiceMock = IModManageService.Mock();
        var service = CreateService(chartServiceMock, modServiceMock);
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash,
            ChartIds = [13]
        };

        await service.ImportAsync(package);

        modServiceMock.InitializeModsAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task ImportAsync_WrongGame_Throws()
    {
        var service = CreateService();
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash2,
            ChartIds = [13]
        };

        Task Act() => service.ImportAsync(package);

        await Assert.That(Act).Throws<InvalidOperationException>();
    }
}
