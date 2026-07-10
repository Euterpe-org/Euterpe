namespace Euterpe.Tests.Core;

[Category("MessagePackSerializationServiceTests")]
[TestSubject(typeof(MessagePackSerializationService))]
public sealed class MessagePackSerializationServiceTest
{
    private readonly MessagePackSerializationService _messagePackSerializationService = new();

    [Test]
    public async Task GameSharePackage_Roundtrip_PreservesAllFields()
    {
        var package = new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameId.MuseDash2,
            ChartIds = [1, 42, 1337],
            Mods =
            [
                new GameShareMod { Name = "CustomAlbums", IsDisabled = false },
                new GameShareMod { Name = "PianoKey", IsDisabled = true }
            ]
        };

        var restored = _messagePackSerializationService.DeserializeGameSharePackage(
            _messagePackSerializationService.SerializeGameSharePackage(package));

        await Assert.That(restored.SchemaVersion).IsEqualTo(GameSharePackage.CurrentSchemaVersion);
        await Assert.That(restored.GameId).IsEqualTo(GameId.MuseDash2);
        await Assert.That(restored.ChartIds).IsEquivalentTo(package.ChartIds);
        await Assert.That(restored.Mods[0].Name).IsEqualTo("CustomAlbums");
        await Assert.That(restored.Mods[0].IsDisabled).IsFalse();
        await Assert.That(restored.Mods[1].Name).IsEqualTo("PianoKey");
        await Assert.That(restored.Mods[1].IsDisabled).IsTrue();
    }
}
