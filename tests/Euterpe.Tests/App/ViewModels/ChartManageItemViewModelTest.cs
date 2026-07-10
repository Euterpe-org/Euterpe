using Euterpe.Features.Charting;

namespace Euterpe.Tests.App.ViewModels;

[Category("ChartManageItemViewModelTests")]
[TestSubject(typeof(ChartManageItemViewModel))]
public sealed class ChartManageItemViewModelTest
{
    [Test]
    public async Task IsSelected_Default_IsFalse()
    {
        var item = new ChartManageItemViewModel(new ChartDto
        {
            FolderPath = "/charts/chart",
            FolderName = "chart",
            Manifest = null!
        });

        await Assert.That(item.IsSelected).IsFalse();
    }
}
