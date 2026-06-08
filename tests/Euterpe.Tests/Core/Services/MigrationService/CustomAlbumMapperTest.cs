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

        var meta = CustomAlbumMapper.ToManifestMeta(info, 0.5f);

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

        var meta = CustomAlbumMapper.ToManifestMeta(info, null);

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

        var meta = CustomAlbumMapper.ToManifestMeta(info, null);

        await Assert.That(meta.Bpm).IsEqualTo(expected);
        await Assert.That(meta.BpmMin).IsEqualTo(expectedMin);
        await Assert.That(meta.BpmMax).IsEqualTo(expectedMax);
    }

    [Test]
    public async Task ToManifestMeta_BuildsMapsForNonBlankDifficultiesWithDesignerFallback()
    {
        var info = new InfoJson
        {
            Name = "Song",
            Author = "Composer",
            Difficulty1 = "2",
            Difficulty2 = "",
            Difficulty3 = "8",
            LevelDesigner = "General",
            LevelDesigner1 = "Alice",
            LevelDesigner3 = ""
        };

        var meta = CustomAlbumMapper.ToManifestMeta(info, null);

        await Assert.That(meta.Maps.Count).IsEqualTo(2);
        await Assert.That(meta.Maps["map1.bms"].Rating).IsEqualTo("2");
        await Assert.That(meta.Maps["map1.bms"].Charters).IsEquivalentTo(new[] { "Alice" });
        await Assert.That(meta.Maps["map3.bms"].Charters).IsEquivalentTo(new[] { "General" });
    }
}
