using DynamicData;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task DeleteChartsAsync_AllSucceed_ReturnsCountAndRemovesFromCache()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("a", ChartSource.Online, "/online/1"));
        local.Set(CreateChart("b", ChartSource.Online, "/online/2"));

        var fileSystem = IFileSystemService.Mock();
        fileSystem.TryDeleteDirectory(Any<string>(), Any<DeleteOption>()).Returns(true);

        var service = CreateService(local, fileSystemService: fileSystem);
        await service.InitializeChartsAsync();
        using var _ = service.Connect().Bind(out var charts).Subscribe();

        var deleted = await service.DeleteChartsAsync(["/online/1", "/online/2"]);

        using var __ = Assert.Multiple();
        await Assert.That(deleted).IsEqualTo(2);
        await Assert.That(charts.Count).IsEqualTo(0);
    }

    [Test]
    public async Task DeleteChartsAsync_SomeFail_ReturnsPartialCount()
    {
        var local = new FakeChartLocalService();
        local.Set(CreateChart("a", ChartSource.Online, "/online/1"));
        local.Set(CreateChart("b", ChartSource.Online, "/online/2"));

        var fileSystem = IFileSystemService.Mock();
        fileSystem.TryDeleteDirectory("/online/1", Any<DeleteOption>()).Returns(true);
        fileSystem.TryDeleteDirectory("/online/2", Any<DeleteOption>()).Returns(false);

        var service = CreateService(local, fileSystemService: fileSystem);
        await service.InitializeChartsAsync();

        var deleted = await service.DeleteChartsAsync(["/online/1", "/online/2"]);

        await Assert.That(deleted).IsEqualTo(1);
    }
}
