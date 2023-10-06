using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.Models;

public class Chart
{
    [JsonPropertyName("analytics")]
    public Analytic Analytics { get; set; }

    [JsonPropertyName("_id")]
    public string IdStr { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("bpm")]
    public string Bpm { get; set; }

    [JsonPropertyName("difficulties")]
    public string[] Difficulties { get; set; }

    [JsonPropertyName("charter")]
    public string Charter { get; set; }

    [JsonPropertyName("charter_id")]
    public string[] CharterId { get; set; }

    [JsonPropertyName("__v")]
    public int V { get; set; }
}

public class Analytic
{
    [JsonPropertyName("likes")]
    public string[] Likes { get; set; }

    [JsonPropertyName("plays")]
    public int Plays { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }
}