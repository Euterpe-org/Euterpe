using System.Text.Json;

namespace Euterpe.Tests.Charts;

[Category("InfoJsonTests")]
[TestSubject(typeof(InfoJson))]
public sealed class InfoJsonTest
{
    [Test]
    public async Task Defaults_EmptyStringsAndEmptyTags()
    {
        var info = new InfoJson();

        using var _ = Assert.Multiple();
        await Assert.That(info.Author).IsEqualTo(string.Empty);
        await Assert.That(info.Bpm).IsEqualTo(string.Empty);
        await Assert.That(info.Name).IsEqualTo(string.Empty);
        await Assert.That(info.SearchTags).IsEmpty();
        await Assert.That(info.SearchTagsString).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task SettingProperty_RaisesPropertyChanged()
    {
        var info = new InfoJson();
        var changed = new List<string?>();
        info.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        info.Author = "tester";
        info.Bpm = "120";
        info.Name = "song";

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(InfoJson.Author));
        await Assert.That(changed).Contains(nameof(InfoJson.Bpm));
        await Assert.That(changed).Contains(nameof(InfoJson.Name));
    }

    [Test]
    public async Task JsonRoundTrip_PreservesValues()
    {
        var original = new InfoJson
        {
            Author = "tester",
            Bpm = "120",
            Difficulty1 = "1",
            Difficulty2 = "2",
            Difficulty3 = "3",
            Difficulty4 = "4",
            HideBmsDifficulty = "h",
            HideBmsMessage = "m",
            HideBmsMode = "mode",
            LevelDesigner = "ld",
            LevelDesigner1 = "ld1",
            LevelDesigner2 = "ld2",
            LevelDesigner3 = "ld3",
            LevelDesigner4 = "ld4",
            Name = "song",
            NameRomanized = "song-romanized",
            Scene = "scene",
            SearchTags = ["tag1", "tag2"],
            UnlockLevel = "5"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<InfoJson>(json)!;

        using var _ = Assert.Multiple();
        await Assert.That(deserialized.Author).IsEqualTo(original.Author);
        await Assert.That(deserialized.Bpm).IsEqualTo(original.Bpm);
        await Assert.That(deserialized.Name).IsEqualTo(original.Name);
        await Assert.That(deserialized.NameRomanized).IsEqualTo(original.NameRomanized);
        await Assert.That(deserialized.SearchTags).IsEquivalentTo(original.SearchTags, EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(deserialized.UnlockLevel).IsEqualTo(original.UnlockLevel);
    }

    [Test]
    public async Task SearchTagsString_IgnoredInJson()
    {
        var info = new InfoJson { SearchTagsString = "ignored" };
        var json = JsonSerializer.Serialize(info);
        await Assert.That(json).DoesNotContain("ignored");
    }
}