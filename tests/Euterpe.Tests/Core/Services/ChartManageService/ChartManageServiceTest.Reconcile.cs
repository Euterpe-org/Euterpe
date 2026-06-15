using DynamicData;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task ReconcileChartsAsync_NewFolderOnDisk_AddsChart()
    {
        var local = new MutableChartLocalService();
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();
        await Assert.That(charts.Count).IsEqualTo(0);

        local.Set("/charts/A", ChartAt("/charts/A", "A"));
        await service.ReconcileChartsAsync();

        using var _ = Assert.Multiple();
        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("A");
    }

    [Test]
    public async Task ReconcileChartsAsync_FolderRemovedFromDisk_PrunesChart()
    {
        var local = new MutableChartLocalService();
        local.Set("/charts/A", ChartAt("/charts/A", "A"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();
        await Assert.That(charts.Count).IsEqualTo(1);

        local.Remove("/charts/A");
        await service.ReconcileChartsAsync();

        await Assert.That(charts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ReconcileChartsAsync_UnchangedLibrary_DoesNotDuplicate()
    {
        var local = new MutableChartLocalService();
        local.Set("/charts/A", ChartAt("/charts/A", "A"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();

        await service.ReconcileChartsAsync();

        await Assert.That(charts.Count).IsEqualTo(1);
    }

    private static ChartDto ChartAt(string folder, string name) =>
        new()
        {
            FolderPath = folder,
            FolderName = Path.GetFileName(folder),
            Source = ChartSource.Offline,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Meta = new ManifestMeta { Name = name, Author = "author", Scene = "scene", Maps = new() },
                Files = new()
            }
        };

    private sealed class MutableChartLocalService : IChartLocalService
    {
        private readonly Dictionary<string, ChartDto> _offline = [];

        public void Set(string folder, ChartDto chart) => _offline[folder] = chart;

        public void Remove(string folder) => _offline.Remove(folder);

        public IEnumerable<string> GetChartFolderPaths(ChartSource source) =>
            source is ChartSource.Offline ? _offline.Keys : [];

        public Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source) =>
            Task.FromResult(_offline.GetValueOrDefault(chartFolder));

        public CustomAlbumSource CreateCustomAlbumSource(string path) => throw new NotSupportedException();

        public CustomAlbumSource[] GetCustomAlbumsSources() => throw new NotSupportedException();
    }
}
