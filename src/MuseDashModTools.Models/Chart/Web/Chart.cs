namespace MuseDashModTools.Models;

[PublicAPI]
public sealed class Chart
{
    [JsonPropertyName("analytics")]
    public Analytic Analytics { get; set; } = null!;

    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("title_romanized")]
    public string? TitleRomanized { get; set; }

    [JsonPropertyName("artist")]
    public string Artist { get; set; } = string.Empty;

    [JsonPropertyName("charter")]
    public string Charter { get; set; } = string.Empty;

    [JsonPropertyName("bpm")]
    public string Bpm { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public float Length { get; set; }

    [JsonPropertyName("owner_uid")]
    public int OwnerUid { get; set; }

    [JsonPropertyName("sheets")]
    public Sheet[] Sheets { get; set; } = [];

    [JsonPropertyName("ranked")]
    public bool Ranked { get; set; }

    [JsonPropertyName("searchTags")]
    public string[] SearchTags { get; set; } = [];

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("__v")]
    public int V { get; set; }
}