using System.Buffers.Text;

namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
    [Test]
    public async Task TryParseShareLink_SurroundingChatText_ExtractsLink()
    {
        var service = CreateService();

        var parsed = service.TryParseShareLink($"试试我的配置 {service.CreateChartShareLink([42])} 谢谢");

        await Assert.That(parsed!.ChartIds).IsEquivalentTo(new[] { 42 });
    }

    [Test]
    public async Task TryParseShareLink_Garbage_ReturnsNull()
    {
        var service = CreateService();

        await Assert.That(service.TryParseShareLink("hello world")).IsNull();
    }

    [Test]
    public async Task TryParseShareLink_UnsupportedSchema_ReturnsNull()
    {
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion + 1,
            GameId = GameId.MuseDash,
            ChartIds = [13]
        };

        await Assert.That(ParseSerializedPackage(package)).IsNull();
    }

    [Test]
    public async Task TryParseShareLink_EmptyPackage_ReturnsNull()
    {
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash
        };

        await Assert.That(ParseSerializedPackage(package)).IsNull();
    }

    [Test]
    public async Task TryParseShareLink_DuplicateChartIds_ReturnsNull()
    {
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash,
            ChartIds = [13, 13]
        };

        await Assert.That(ParseSerializedPackage(package)).IsNull();
    }

    private static GameSharePackage? ParseSerializedPackage(GameSharePackage package)
    {
        var serialization = new MessagePackSerializationService();
        var code = Base64Url.EncodeToString(serialization.SerializeGameSharePackage(package));
        return CreateService().TryParseShareLink(code);
    }
}
