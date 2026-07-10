namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
    [Test]
    public async Task CreateChartShareLink_ThenParse_RoundTripsChartIds()
    {
        var service = CreateService();

        var parsed = service.TryParseShareLink(service.CreateChartShareLink([13, 20]));

        using var _ = Assert.Multiple();
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed!.GameId).IsEqualTo(GameId.MuseDash);
        await Assert.That(parsed.ChartIds).IsEquivalentTo(new[] { 13, 20 });
        await Assert.That(parsed.Mods).IsEmpty();
    }

    [Test]
    public async Task CreateChartShareLink_DuplicateChartIds_Deduplicates()
    {
        var service = CreateService();

        var parsed = service.TryParseShareLink(service.CreateChartShareLink([13, 13, 20]));

        await Assert.That(parsed!.ChartIds).IsEquivalentTo(new[] { 13, 20 });
    }

    [Test]
    public async Task CreateChartShareLink_InvalidChartIds_Throws()
    {
        var service = CreateService();

        void Act() => service.CreateChartShareLink([0]);

        await Assert.That(Act).Throws<ArgumentException>();
    }

    [Test]
    public async Task CreateChartShareLink_TooManyChartIds_Throws()
    {
        var service = CreateService();
        var chartIds = Enumerable.Range(1, GameSharePackage.MaximumChartCount + 1).ToArray();

        void Act() => service.CreateChartShareLink(chartIds);

        await Assert.That(Act).Throws<ArgumentException>();
    }
}
