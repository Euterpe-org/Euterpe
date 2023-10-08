using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Models;

public class Chart
{
    [JsonPropertyName("analytics")]
    public Analytic Analytics { get; set; }

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
    public string[] Difficulties { get; set; }

    [JsonPropertyName("charter")]
    public string Charter { get; set; } = string.Empty;

    [JsonPropertyName("charter_id")]
    public string[] CharterId { get; set; }

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

    [JsonIgnore]
    public bool HasEasy => EasyLevel != "0";

    [JsonIgnore]
    public bool HasHard => HardLevel != "0";

    [JsonIgnore]
    public bool HasMaster => MasterLevel != "0";

    [JsonIgnore]
    public bool HasHidden => HiddenLevel != "0";
}

public class Analytic
{
    [JsonPropertyName("likes")]
    public string[] Likes { get; set; }

    [JsonIgnore]
    public int LikesCount => Likes.Length;

    [JsonPropertyName("plays")]
    public int Plays { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }
}