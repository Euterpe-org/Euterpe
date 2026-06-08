using Euterpe.Core;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Tests;

[Category("CustomAlbumMapperTests")]
[TestSubject(typeof(CustomAlbumMapper))]
public sealed class CustomAlbumMapperTest
{
    [Test]
    public async Task ToManifestMeta_MapsCoreFields()
    {
        var info = new InfoJson
        {
            Name = "Song",
            NameRomanized = "Song Romanized",
            Author = "Composer",
            Scene = "scene_01"
        };

        var meta = CustomAlbumMapper.ToManifestMeta(info, 0.5f, []);

        await Assert.That(meta.Name).IsEqualTo("Song");
        await Assert.That(meta.NameRomanized).IsEqualTo("Song Romanized");
        await Assert.That(meta.Author).IsEqualTo("Composer");
        await Assert.That(meta.Scene).IsEqualTo("scene_01");
        await Assert.That(meta.BackgroundVideoOpacity).IsEqualTo(0.5f);
    }

    [Test]
    public async Task ToManifestMeta_BlankOptionalFieldsPassThrough()
    {
        var info = new InfoJson { Name = "Song", Author = "Composer" };

        var meta = CustomAlbumMapper.ToManifestMeta(info, null, []);

        await Assert.That(meta.NameRomanized).IsEqualTo(string.Empty);
        await Assert.That(meta.HideMode).IsEqualTo(string.Empty);
        await Assert.That(meta.HideRatingOverride).IsEqualTo(string.Empty);
        await Assert.That(meta.HideMessage).IsEqualTo(string.Empty);
        await Assert.That(meta.SearchKeywords).IsEquivalentTo(Array.Empty<string>());
        await Assert.That(meta.BackgroundVideoOpacity).IsNull();
    }

    [Test]
    [Arguments("120", 120, null, null)]
    [Arguments("120~140", 130, 120, 140)]
    [Arguments("", 0, null, null)]
    [Arguments("not-a-number", 0, null, null)]
    public async Task ToManifestMeta_ParsesBpm(string bpm, int expected, int? expectedMin, int? expectedMax)
    {
        var info = new InfoJson { Name = "Song", Author = "Composer", Bpm = bpm };

        var meta = CustomAlbumMapper.ToManifestMeta(info, null, []);

        await Assert.That(meta.Bpm).IsEqualTo(expected);
        await Assert.That(meta.BpmMin).IsEqualTo(expectedMin);
        await Assert.That(meta.BpmMax).IsEqualTo(expectedMax);
    }

    [Test]
    public async Task ToManifestMeta_BuildsMapsForGivenDifficultiesWithDesignerFallback()
    {
        var info = new InfoJson
        {
            Name = "Song",
            Author = "Composer",
            Difficulty1 = "2",
            Difficulty2 = "5", // declared in info.json but Hard is not among the present difficulties, so it must be excluded
            Difficulty3 = "8",
            LevelDesigner = "General",
            LevelDesigner1 = "Alice",
            LevelDesigner3 = ""
        };

        var meta = CustomAlbumMapper.ToManifestMeta(info, null, [ChartDifficulty.Easy, ChartDifficulty.Master]);

        await Assert.That(meta.Maps.Count).IsEqualTo(2);
        await Assert.That(meta.Maps.ContainsKey("map2")).IsFalse();
        await Assert.That(meta.Maps["map1"].Rating).IsEqualTo("2");
        await Assert.That(meta.Maps["map1"].Charters).IsEquivalentTo(new[] { "Alice" });
        await Assert.That(meta.Maps["map3"].Rating).IsEqualTo("8");
        await Assert.That(meta.Maps["map3"].Charters).IsEquivalentTo(new[] { "General" });
    }
}
