using System.Buffers.Text;

namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
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
            SchemaVersion = Manifest.CurrentSchema + 1,
            GameId = GameId.MuseDash,
            ChartIds = [13]
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
