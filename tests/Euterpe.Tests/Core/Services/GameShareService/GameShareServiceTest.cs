namespace Euterpe.Tests.Core;

[Category("GameShareServiceTests")]
[TestSubject(typeof(GameShareService))]
public sealed partial class GameShareServiceTest
{
    private static GameShareService CreateService(
        IChartManageService? chartManageService = null,
        IModManageService? modManageService = null) =>
        new()
        {
            ChartManageService = chartManageService ?? IChartManageService.Mock(),
            ModManageService = modManageService ?? IModManageService.Mock(),
            MessagePackSerialization = new MessagePackSerializationService(),
            GameConfig = new MuseDashConfig()
        };

    private static ModDto CreateRemoteMod(string name, bool isDisabled)
    {
        var mod = new ModDto { Name = name, FileName = $"{name}.dll", Version = "1.0.0" };
        mod.AddLocalInfo();
        mod.IsDisabled = isDisabled;
        return mod;
    }

    private static ModDto CreateLocalOnlyMod(string name) =>
        new() { Name = name, FileNameWithoutExtension = name };
}
