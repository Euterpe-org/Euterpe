namespace Euterpe.Models.Charts;

public static class ChartRating
{
    public static double Parse(string? rating)
    {
        if (rating.IsNullOrEmpty())
        {
            return -1;
        }

        var plus = rating.EndsWith('+');
        var numeric = plus ? rating[..^1] : rating;
        return double.TryParse(numeric, out var value) ? value + (plus ? 0.5 : 0) : -1;
    }
}
