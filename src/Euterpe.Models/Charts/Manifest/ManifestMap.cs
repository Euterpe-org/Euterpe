using System.Globalization;
using PolyType;

namespace Euterpe.Models.Charts;

public sealed class ManifestMap
{
    [PropertyShape(Name = "rating")] public string Rating { get; set; } = null!;
    [PropertyShape(Name = "charters")] public string[] Charters { get; set; } = null!;
    [PropertyShape(Name = "predicted_rating")] public double? PredictedRating { get; set; }

    [PropertyShape(Ignore = true)]
    public double RatingValue =>
        double.TryParse(Rating, CultureInfo.InvariantCulture, out var value) ? value : -1;
}
