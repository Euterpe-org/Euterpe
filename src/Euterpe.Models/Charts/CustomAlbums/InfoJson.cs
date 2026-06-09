namespace Euterpe.Models.Charts.CustomAlbums;

public sealed class InfoJson
{
    public string Author { get; set; } = string.Empty;
    public string Bpm { get; set; } = string.Empty;
    public string Difficulty1 { get; set; } = string.Empty;
    public string Difficulty2 { get; set; } = string.Empty;
    public string Difficulty3 { get; set; } = string.Empty;
    public string Difficulty4 { get; set; } = string.Empty;
    public string HideBmsMessage { get; set; } = string.Empty;
    public string HideBmsMode { get; set; } = string.Empty;
    public string LevelDesigner { get; set; } = string.Empty;
    public string LevelDesigner1 { get; set; } = string.Empty;
    public string LevelDesigner2 { get; set; } = string.Empty;
    public string LevelDesigner3 { get; set; } = string.Empty;
    public string LevelDesigner4 { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_romanized")]
    public string NameRomanized { get; set; } = string.Empty;

    public string Scene { get; set; } = string.Empty;
    public string[] SearchTags { get; set; } = [];
    public string UnlockLevel { get; set; } = string.Empty;
    public bool Streamer { get; set; }
}
