using DynamicData;
using Euterpe.Contracts.Charts;
using Euterpe.Models.Progress;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task DownloadChartAsync_ChartAlreadyInstalled_RoutesToUpdate()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("installed", ChartSource.Online, "/online/13"));

        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileSizes(Any<string>()).Returns(new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));

        var download = IGameDownloadManager.Mock();
        download.CheckChartUpdatesAsync(Any<CheckChartUpdatesRequest>(), Any<CancellationToken>())
            .Returns(new CheckChartUpdatesResponse());

        var service = CreateService(local, download, fileSystem);
        await service.InitializeChartsAsync();

        await service.DownloadChartAsync("13");

        using var _ = Assert.Multiple();
        download.CheckChartUpdatesAsync(Any<CheckChartUpdatesRequest>(), Any<CancellationToken>()).WasCalled(Times.Once);
        download.DownloadChartAsync(Any<string>(), Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task DownloadChartAsync_ChartNotInstalled_DownloadsThroughDownloadManager()
    {
        var local = new FakeChartLocalService();

        var download = IGameDownloadManager.Mock();
        download.DownloadChartAsync("13", Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).Returns("/online/13");

        var service = CreateService(local, download);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Set(CreateChart("downloaded", ChartSource.Online, "/online/13"));
        await service.DownloadChartAsync("13", new Progress<BatchProgress>());

        using var __ = Assert.Multiple();
        download.DownloadChartAsync("13", Any<IProgress<BatchProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Once);
        download.CheckChartUpdatesAsync(Any<CheckChartUpdatesRequest>(), Any<CancellationToken>()).WasCalled(Times.Never);
        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("downloaded");
    }
}
