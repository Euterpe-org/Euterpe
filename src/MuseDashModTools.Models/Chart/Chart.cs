using Avalonia.Media.Imaging;

namespace MuseDashModTools.Models;

public sealed class Chart
{
    [JsonPropertyName("analytics")]
    public Analytic Analytics { get; set; } = null!;

    [JsonPropertyName("_id")]
    public string IdStr { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("bpm")]
    public string Bpm { get; set; } = string.Empty;

    [JsonPropertyName("difficulties")]
    public string[] Difficulties { get; set; } = [];

    [JsonPropertyName("charter")]
    public string Charter { get; set; } = string.Empty;

    [JsonPropertyName("charter_id")]
    public string[] CharterId { get; set; } = [];

    [JsonPropertyName("__v")]
    public int V { get; set; }

    [JsonIgnore]
    public Bitmap? Cover { get; set; }

    [JsonIgnore]
    public string EasyLevel => Difficulties[0];

    [JsonIgnore]
    public string HardLevel => Difficulties[1];

    [JsonIgnore]
    public string MasterLevel => Difficulties[2];

    [JsonIgnore]
    public string HiddenLevel => Difficulties[3];

    public int GetHighestLevel()
    {
        var highestLevel = Difficulties.Max(x => x.ParseLevel());
        return highestLevel;
    }
}