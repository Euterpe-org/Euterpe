using System.Globalization;
using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class ManifestMap
{
    [Key("rating")] public string Rating { get; set; } = null!;
    [Key("charters")] public string[] Charters { get; set; } = null!;
    [Key("predicted_rating")] public double? PredictedRating { get; set; }

    [IgnoreMember]
    public double RatingValue =>
        double.TryParse(Rating, CultureInfo.InvariantCulture, out var value) ? value : -1;
}
