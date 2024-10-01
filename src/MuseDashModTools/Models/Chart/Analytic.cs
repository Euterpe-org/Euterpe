using System.Text.Json.Serialization;

namespace MuseDashModTools.Models.Chart;

[UsedImplicitly]
public sealed class Analytic
{
    [JsonPropertyName("likes")]
    public string[] Likes { get; set; } = [];

    [JsonIgnore]
    public int LikesCount => Likes.Length;

    [JsonPropertyName("plays")]
    public int Plays { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }
}