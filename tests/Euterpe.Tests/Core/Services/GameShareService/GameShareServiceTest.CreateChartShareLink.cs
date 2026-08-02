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
    }
}
