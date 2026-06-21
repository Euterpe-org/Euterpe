using DynamicData;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task ReconcileChartsAsync_NewFolderOnDisk_AddsChart()
    {
        var local = new FakeChartLocalService();
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();
        await Assert.That(charts.Count).IsEqualTo(0);

        local.Set(CreateChart("A"));
        await service.ReconcileChartsAsync();

        using var _ = Assert.Multiple();
        await Assert.That(charts.Count).IsEqualTo(1);
        await Assert.That(charts[0].Manifest.Meta.Name).IsEqualTo("A");
    }

    [Test]
    public async Task ReconcileChartsAsync_FolderRemovedFromDisk_PrunesChart()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("A"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();
        await Assert.That(charts.Count).IsEqualTo(1);

        local.Remove(ChartFolder);
        await service.ReconcileChartsAsync();

        await Assert.That(charts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ReconcileChartsAsync_UnchangedLibrary_DoesNotDuplicate()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("A"));
        var service = CreateService(local);
        await service.InitializeChartsAsync();
        using var connection = service.Connect().Bind(out var charts).Subscribe();

        await service.ReconcileChartsAsync();

        await Assert.That(charts.Count).IsEqualTo(1);
    }
}
