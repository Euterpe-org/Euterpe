using DynamicData;
using Euterpe.Contracts.Charts;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task UpdateAllChartsAsync_ManifestInDeleted_PrunesTombstonedChart()
    {
        var local = new OnlineChartLocalService();
        local.Set("/online/13", OnlineChartAt("/online/13", "Song"));

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
        var local = new OnlineChartLocalService();
        local.Set("/online/13", OnlineChartAt("/online/13", "before"));

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

        local.Set("/online/13", OnlineChartAt("/online/13", "after"));
        await service.UpdateAllChartsAsync();

        using var __ = Assert.Multiple();
        download.UpdateChartAsync("13", Any<IReadOnlyCollection<string>>(), Any<IReadOnlyCollection<string>>(), Any<CancellationToken>()).WasCalled(Times.Once);
        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("after");
    }

    private static ChartDto OnlineChartAt(string folder, string name) =>
        new()
        {
            FolderPath = folder,
            FolderName = Path.GetFileName(folder),
            Source = ChartSource.Online,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Meta = new ManifestMeta { Name = name, Author = "author", Scene = "scene", Maps = new() },
                Files = new()
            }
        };

    private sealed class OnlineChartLocalService : IChartLocalService
    {
        private readonly Dictionary<string, ChartDto> _online = [];

        public void Set(string folder, ChartDto chart) => _online[folder] = chart;

        public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
            source is ChartSource.Online ? _online.Keys : [];

        public Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source) =>
            Task.FromResult(_online.GetValueOrDefault(chartFolder));

        public CustomAlbumSource CreateCustomAlbumSource(string path) => throw new NotSupportedException();

        public CustomAlbumSource[] GetCustomAlbumsSources() => throw new NotSupportedException();
    }
}
