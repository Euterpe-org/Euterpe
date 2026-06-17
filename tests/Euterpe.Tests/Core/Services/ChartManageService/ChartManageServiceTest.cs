using DynamicData;
using Euterpe.Models.Charts.CustomAlbums;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("ChartManageServiceTests")]
[TestSubject(typeof(ChartManageService))]
public sealed partial class ChartManageServiceTest
{
    private const string ChartFolder = "/charts/A";

    [Test]
    public async Task RefreshChartAsync_KnownFolder_ReloadsManifestFromDisk()
    {
        var local = new FakeChartLocalService(ChartFolder) { Chart = CreateChart("before") };
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Chart = CreateChart("after");
        await service.RefreshChartAsync(ChartFolder);

        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("after");
    }

    [Test]
    public async Task RefreshChartAsync_FolderOutsideLibrary_LeavesLibraryUnchanged()
    {
        var local = new FakeChartLocalService(ChartFolder) { Chart = CreateChart("before") };
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Chart = CreateChart("after");
        await service.RefreshChartAsync("/somewhere/else");

        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("before");
    }

    private static ChartManageService CreateService(IChartLocalService localService) =>
        new()
        {
            GameConfig = new MuseDashConfig(),
            Archive = IArchiveService.Mock(),
            ChartLocalService = localService,
            FileSystemService = IFileSystemService.Mock(),
            GameDownloadManager = IGameDownloadManager.Mock(),
            Logger = Mock.Logger<ChartManageService>(),
            NotificationService = INotificationService.Mock(),
            MigrationService = IMigrationService.Mock()
        };

    private static ChartDto CreateChart(string name) =>
        new()
        {
            FolderPath = ChartFolder,
            FolderName = "A",
            Source = ChartSource.Offline,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Meta = new ManifestMeta { Name = name, Author = "author", Scene = "scene", Maps = new() },
                Files = new()
            }
        };

    private sealed class FakeChartLocalService(string offlineFolder) : IChartLocalService
    {
        public ChartDto? Chart { get; set; }

        public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
            source is ChartSource.Offline ? [offlineFolder] : [];

        public Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source) =>
            Task.FromResult(Chart);

        public CustomAlbumSource CreateCustomAlbumSource(string path) => throw new NotSupportedException();

        public CustomAlbumSource[] GetCustomAlbumsSources() => throw new NotSupportedException();
    }
}
