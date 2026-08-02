using Euterpe.Models.Progress;

namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
    [Test]
    public async Task ImportAsync_Charts_DelegatesAndReturnsResults()
    {
        var chartServiceMock = IChartManageService.Mock();
        chartServiceMock.DownloadChartsAsync(Any<IReadOnlyList<string>>(), Any<IProgress<BatchProgress>?>(), Any<CancellationToken>())
            .Returns([new BulkItemResult("13", BulkItemOutcome.Added)]);
        var service = CreateService(chartServiceMock);
        var package = new GameSharePackage
        {
            GameId = GameId.MuseDash,
            ChartIds = [13]
        };

        var result = await service.ImportAsync(package);

        await Assert.That(result.Single().Outcome).IsEqualTo(BulkItemOutcome.Added);
        chartServiceMock.InitializeChartsAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task ImportAsync_WrongGame_Throws()
    {
        var service = CreateService();
        var package = new GameSharePackage
        {
            GameId = GameId.MuseDash2,
            ChartIds = [13]
        };

        Task Act()
        {
            return service.ImportAsync(package);
        }

        await Assert.That(Act).Throws<InvalidOperationException>();
    }
}
