using System.Text.Json.Serialization;

namespace MuseDashModTools.Models;

public sealed partial class InfoJson : ObservableObject
{
    [JsonPropertyName("author")]
    [ObservableProperty] private string _author = string.Empty;

    [JsonPropertyName("bpm")]
    [ObservableProperty] private string _bpm = string.Empty;

    [JsonPropertyName("difficulty1")]
    [ObservableProperty] private string _difficulty1 = string.Empty;

    [JsonPropertyName("difficulty2")]
    [ObservableProperty] private string _difficulty2 = string.Empty;

    [JsonPropertyName("difficulty3")]
    [ObservableProperty] private string _difficulty3 = string.Empty;

    [JsonPropertyName("difficulty4")]
    [ObservableProperty] private string _difficulty4 = string.Empty;

    [JsonPropertyName("hideBmsDifficulty")]
    [ObservableProperty] private string _hideBmsDifficulty = string.Empty;

    [JsonPropertyName("hideBmsMessage")]
    [ObservableProperty] private string _hideBmsMessage = string.Empty;

    [JsonPropertyName("hideBmsMode")]
    [ObservableProperty] private string _hideBmsMode = string.Empty;

    [JsonPropertyName("levelDesigner")]
    [ObservableProperty] private string _levelDesigner = string.Empty;

    [JsonPropertyName("levelDesigner1")]
    [ObservableProperty] private string _levelDesigner1 = string.Empty;

    [JsonPropertyName("levelDesigner2")]
    [ObservableProperty] private string _levelDesigner2 = string.Empty;

    [JsonPropertyName("levelDesigner3")]
    [ObservableProperty] private string _levelDesigner3 = string.Empty;

    [JsonPropertyName("levelDesigner4")]
    [ObservableProperty] private string _levelDesigner4 = string.Empty;

    [JsonPropertyName("name")]
    [ObservableProperty] private string _name = string.Empty;

    [JsonPropertyName("name_romanized")]
    [ObservableProperty] private string _nameRomanized = string.Empty;

    [JsonPropertyName("scene")]
    [ObservableProperty] private string _scene = string.Empty;

    [JsonPropertyName("searchTags")]
    [ObservableProperty] private string[] _searchTags = Array.Empty<string>();

    [JsonIgnore]
    [ObservableProperty] private string _searchTagsString = string.Empty;

    [JsonPropertyName("unlockLevel")]
    [ObservableProperty] private string _unlockLevel = string.Empty;
}