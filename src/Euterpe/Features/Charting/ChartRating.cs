namespace Euterpe.Features.Charting;

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

    public static double Max(ChartDto chart) =>
        chart.Manifest.Meta.Maps.Values.Select(m => Parse(m.Rating)).DefaultIfEmpty(-1).Max();

    public static string MaxDisplay(ChartDto chart) =>
        chart.Manifest.Meta.Maps.Values
            .Select(m => m.Rating)
            .Where(r => !r.IsNullOrEmpty())
            .OrderByDescending(Parse)
            .FirstOrDefault() ?? "?";

    public static string BpmDisplay(ManifestMeta meta) =>
        meta is { BpmMin: { } min, BpmMax: { } max } && min != max
            ? $"{min}–{max}"
            : meta.Bpm.ToString(CultureInfo.InvariantCulture);
}
