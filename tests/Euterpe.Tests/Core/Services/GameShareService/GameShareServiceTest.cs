namespace Euterpe.Tests.Core;

[Category("GameShareServiceTests")]
[TestSubject(typeof(GameShareService))]
public sealed partial class GameShareServiceTest
{
    private static GameShareService CreateService(IChartManageService? chartManageService = null) =>
        new()
        {
            ChartManageService = chartManageService ?? IChartManageService.Mock(),
            MessagePackSerialization = new MessagePackSerializationService(),
            GameConfig = new MuseDashConfig()
        };
}
