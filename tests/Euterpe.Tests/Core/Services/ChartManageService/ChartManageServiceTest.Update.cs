using DynamicData;
using Euterpe.Contracts.Charts;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task UpdateAllChartsAsync_ManifestInDeleted_PrunesTombstonedChart()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("Song", ChartSource.Online, "/online/13"));

        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase) { ["manifest.epk"] = 1 });
        fileSystem.TryDeleteDirectory(Any<string>(), Any<DeleteOption>()).Returns(true);

        var download = IGameDownloadManager.Mock();
        download.CheckChartUpdatesAsync(Any<CheckChartUpdatesRequest>(), Any<CancellationToken>())
            .Returns(new CheckChartUpdatesResponse
            {
                Charts = { ["13"] = new ChartUpdateDelta { Deleted = ["manifest.epk"] } }
            });

        var service = CreateService(local, download, fileSystem);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();
        await Assert.That(charts.Count).IsEqualTo(1);

        await service.UpdateAllChartsAsync();

        await Assert.That(charts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task UpdateAllChartsAsync_ChangedAndDeletedDelta_UpdatesThroughDownloadManager()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("before", ChartSource.Online, "/online/13"));

        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["manifest.epk"] = 1,
            ["music.ogg"] = 2,
            ["cover.png"] = 3
        });

        var download = IGameDownloadManager.Mock();
        download.CheckChartUpdatesAsync(Any<CheckChartUpdatesRequest>(), Any<CancellationToken>())
            .Returns(new CheckChartUpdatesResponse
            {
                Charts = { ["13"] = new ChartUpdateDelta { Changed = ["music.ogg"], Deleted = ["cover.png"] } }
            });
        download.UpdateChartAsync("13", Any<IReadOnlyCollection<string>>(), Any<IReadOnlyCollection<string>>(), Any<CancellationToken>())
            .Returns("/online/13");

        var service = CreateService(local, download, fileSystem);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Set(CreateChart("after", ChartSource.Online, "/online/13"));
        await service.UpdateAllChartsAsync();

        using var __ = Assert.Multiple();
        download.UpdateChartAsync("13", Any<IReadOnlyCollection<string>>(), Any<IReadOnlyCollection<string>>(), Any<CancellationToken>()).WasCalled(Times.Once);
        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("after");
    }
}
