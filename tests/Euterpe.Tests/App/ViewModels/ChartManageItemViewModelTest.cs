using Euterpe.Features.Charting;

namespace Euterpe.Tests.App.ViewModels;

[Category("ChartManageItemViewModelTests")]
[TestSubject(typeof(ChartManageItemViewModel))]
public sealed class ChartManageItemViewModelTest
{
    [Test]
    [Arguments(ChartSource.Online, 13, true)]
    [Arguments(ChartSource.Online, null, false)]
    [Arguments(ChartSource.Offline, 13, false)]
    public async Task CanShare_SourceAndCid_RequiresOnlineChart(ChartSource source, int? cid, bool expected)
    {
        var item = new ChartManageItemViewModel(CreateChart(source, cid));

        await Assert.That(item.CanShare).IsEqualTo(expected);
    }

    [Test]
    public async Task IsSelected_Default_IsFalse()
    {
        var item = new ChartManageItemViewModel(CreateChart(ChartSource.Online, 13));

        await Assert.That(item.IsSelected).IsFalse();
    }

    private static ChartDto CreateChart(ChartSource source, int? cid) =>
        new()
        {
            FolderPath = "/charts/chart",
            FolderName = "chart",
            Source = source,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Cid = cid,
                Meta = new ManifestMeta
                {
                    Name = "Chart",
                    Author = "Author",
                    Scene = "Scene",
                    Maps = []
                },
                Files = []
            }
        };
}
