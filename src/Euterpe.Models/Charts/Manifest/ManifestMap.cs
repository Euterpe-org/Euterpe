using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class ManifestMap
{
    [Key("rating")] public string Rating { get; set; } = null!;
    [Key("charters")] public string[] Charters { get; set; } = null!;
    [Key("predicted_rating")] public double? PredictedRating { get; set; }
}
