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
    public double RatingValue
    {
        get
        {
            if (Rating.IsNullOrEmpty())
            {
                return -1;
            }

            var plus = Rating.EndsWith('+');
            var numeric = plus ? Rating[..^1] : Rating;
            return double.TryParse(numeric, CultureInfo.InvariantCulture, out var value) ? value + (plus ? 0.5 : 0) : -1;
        }
    }
}
