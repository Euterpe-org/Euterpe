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
            SchemaVersion = Manifest.CurrentSchema,
            GameId = GameId.MuseDash2,
            ChartIds = [1, 42, 1337]
        };

        var restored = _messagePackSerializationService.DeserializeGameSharePackage(
            _messagePackSerializationService.SerializeGameSharePackage(package));

        await Assert.That(restored.SchemaVersion).IsEqualTo(Manifest.CurrentSchema);
        await Assert.That(restored.GameId).IsEqualTo(GameId.MuseDash2);
        await Assert.That(restored.ChartIds).IsEquivalentTo(package.ChartIds);
    }
}
