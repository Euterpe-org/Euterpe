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
        var local = new FakeChartLocalService();
        local.Set(CreateChart("before"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Set(CreateChart("after"));
        await service.RefreshChartAsync(ChartFolder);

        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("after");
    }

    [Test]
    public async Task RefreshChartAsync_FolderOutsideLibrary_LeavesLibraryUnchanged()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("before"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        local.Set(CreateChart("after"));
        await service.RefreshChartAsync("/somewhere/else");

        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("before");
    }

    private static ChartManageService CreateService(
        IChartLocalService localService,
        IGameDownloadManager? gameDownloadManager = null,
        IFileSystemService? fileSystemService = null) =>
        new()
        {
            GameConfig = new MuseDashConfig(),
            Archive = IArchiveService.Mock(),
            ChartLocalService = localService,
            FileSystemService = fileSystemService ?? IFileSystemService.Mock(),
            GameDownloadManager = gameDownloadManager ?? IGameDownloadManager.Mock(),
            Logger = Mock.Logger<ChartManageService>(),
            NotificationService = INotificationService.Mock(),
            MigrationService = IMigrationService.Mock()
        };

    private static ChartDto CreateChart(string name, ChartSource source = ChartSource.Offline, string folderPath = ChartFolder) =>
        new()
        {
            FolderPath = folderPath,
            FolderName = Path.GetFileName(folderPath),
            Source = source,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Meta = new ManifestMeta { Name = name, Author = "author", Scene = "scene", Maps = new() },
                Files = new()
            }
        };

    private sealed class FakeChartLocalService : IChartLocalService
    {
        private readonly Dictionary<string, ChartDto> _charts = [];

        public void Set(ChartDto chart) => _charts[chart.FolderPath] = chart;

        public void Remove(string folderPath) => _charts.Remove(folderPath);

        public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
            _charts.Values.Where(chart => chart.Source == source).Select(chart => chart.FolderPath);

        public Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source) =>
            Task.FromResult(_charts.GetValueOrDefault(chartFolder));

        public CustomAlbumSource CreateCustomAlbumSource(string path) => throw new NotSupportedException();

        public CustomAlbumSource[] GetCustomAlbumsSources() => throw new NotSupportedException();
    }
}
