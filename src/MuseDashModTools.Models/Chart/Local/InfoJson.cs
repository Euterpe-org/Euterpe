namespace MuseDashModTools.Models;

public sealed partial class InfoJson : ObservableObject
{
    [JsonPropertyName("author")]
    [ObservableProperty]
    public partial string Author { get; set; } = string.Empty;

    [JsonPropertyName("bpm")]
    [ObservableProperty]
    public partial string Bpm { get; set; } = string.Empty;

    [JsonPropertyName("difficulty1")]
    [ObservableProperty]
    public partial string Difficulty1 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty2")]
    [ObservableProperty]
    public partial string Difficulty2 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty3")]
    [ObservableProperty]
    public partial string Difficulty3 { get; set; } = string.Empty;

    [JsonPropertyName("difficulty4")]
    [ObservableProperty]
    public partial string Difficulty4 { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsDifficulty")]
    [ObservableProperty]
    public partial string HideBmsDifficulty { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsMessage")]
    [ObservableProperty]
    public partial string HideBmsMessage { get; set; } = string.Empty;

    [JsonPropertyName("hideBmsMode")]
    [ObservableProperty]
    public partial string HideBmsMode { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner")]
    [ObservableProperty]
    public partial string LevelDesigner { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner1")]
    [ObservableProperty]
    public partial string LevelDesigner1 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner2")]
    [ObservableProperty]
    public partial string LevelDesigner2 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner3")]
    [ObservableProperty]
    public partial string LevelDesigner3 { get; set; } = string.Empty;

    [JsonPropertyName("levelDesigner4")]
    [ObservableProperty]
    public partial string LevelDesigner4 { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_romanized")]
    [ObservableProperty]
    public partial string NameRomanized { get; set; } = string.Empty;

    [JsonPropertyName("scene")]
    [ObservableProperty]
    public partial string Scene { get; set; } = string.Empty;

    [JsonPropertyName("searchTags")]
    [ObservableProperty]
    public partial string[] SearchTags { get; set; } = [];

    [JsonIgnore]
    [ObservableProperty]
    public partial string SearchTagsString { get; set; } = string.Empty;

    [JsonPropertyName("unlockLevel")]
    [ObservableProperty]
    public partial string UnlockLevel { get; set; } = string.Empty;
}