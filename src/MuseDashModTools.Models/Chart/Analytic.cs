namespace MuseDashModTools.Models;

[PublicAPI]
public sealed class Analytic
{
    [JsonPropertyName("likes")]
    public int[] Likes { get; set; } = [];

    [JsonPropertyName("plays")]
    public int Plays { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonIgnore]
    public int LikesCount => Likes.Length;
}