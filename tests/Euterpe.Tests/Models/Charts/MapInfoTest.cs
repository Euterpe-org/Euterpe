namespace Euterpe.Tests.Charts;

[Category("MapInfoTests")]
[TestSubject(typeof(MapInfo))]
public sealed class MapInfoTest
{
    [Test]
    public async Task Defaults_AreEmptyStrings()
    {
        var info = new MapInfo();

        using var _ = Assert.Multiple();
        await Assert.That(info.LevelDesigner).IsEqualTo(string.Empty);
        await Assert.That(info.Difficulty).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task SettersStoreValues()
    {
        var info = new MapInfo
        {
            LevelDesigner = "ld",
            Difficulty = "9"
        };

        using var _ = Assert.Multiple();
        await Assert.That(info.LevelDesigner).IsEqualTo("ld");
        await Assert.That(info.Difficulty).IsEqualTo("9");
    }
}