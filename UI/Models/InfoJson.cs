using System.Text.Json.Serialization;

namespace MuseDashModToolsUI.Models;

public class InfoJson
{
    [JsonPropertyName("name")]
    public string ChartName { get; set; } = string.Empty;

    [JsonPropertyName("name_romanized")]
    public string NameRomanized { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("bpm")]
    public string Bpm { get; set; } = string.Empty;

    [JsonPropertyName("scene")]
    public string Scene { get; set; } = string.Empty;

    [JsonPropertyName("searchTags")]
    public string[] SearchTags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("levelDesigner")]
    public string LevelDesigner { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner1")]
    public string LevelDesigner1 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner2")]
    public string LevelDesigner2 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner3")]
    public string LevelDesigner3 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner4")]
    public string LevelDesigner4 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty1")]
    public string Difficulty1 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty2")]
    public string Difficulty2 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty3")]
    public string Difficulty3 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty4")]
    public string Difficulty4 { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsMode")]
    public string HideBmsMode { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsDifficulty")]
    public string HideBmsDifficulty { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsMessage")]
    public string HideBmsMessage { get; set; } = string.Empty;

    [JsonPropertyName("unlockLevel")]
    public string UnlockLevel { get; set; } = string.Empty;
}