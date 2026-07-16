using System.Globalization;
using PolyType;

namespace Euterpe.Models.Charts;

public sealed class ManifestMap
{
    public string Rating { get; set; } = null!;
    public string[] Charters { get; set; } = null!;
    public double? PredictedRating { get; set; }

    [PropertyShape(Ignore = true)]
    public double RatingValue =>
        double.TryParse(Rating, CultureInfo.InvariantCulture, out var value) ? value : -1;
}
