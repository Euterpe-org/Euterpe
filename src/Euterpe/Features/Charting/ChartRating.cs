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

    public static string BpmDisplay(ManifestMeta meta) =>
        meta is { BpmMin: { } min, BpmMax: { } max } && min != max
            ? $"{min}–{max}"
            : meta.Bpm.ToString(CultureInfo.InvariantCulture);

    public static long Size(ChartDto chart) =>
        chart.Manifest.Files.Values.Sum(f => f.Size);

    public static string SizeDisplay(ChartDto chart) =>
        Size(chart) switch
        {
            var b and >= 1 << 30 => string.Create(CultureInfo.InvariantCulture, $"{b / (double)(1 << 30):0.#} GB"),
            var b and >= 1 << 20 => string.Create(CultureInfo.InvariantCulture, $"{b / (double)(1 << 20):0.#} MB"),
            var b => string.Create(CultureInfo.InvariantCulture, $"{b / (double)(1 << 10):0.#} KB")
        };

    // All charters across difficulties, de-duplicated case-insensitively (first spelling wins).
    public static string CharterDisplay(ChartDto chart) =>
        string.Join("、", chart.Manifest.Meta.Maps.Values
            .SelectMany(m => m.Charters)
            .Distinct(StringComparer.OrdinalIgnoreCase));

    public static IReadOnlyList<DifficultyBadge> Badges(ChartDto chart) =>
    [
        .. chart.Difficulties.Select(difficulty =>
            new DifficultyBadge(
                difficulty,
                chart.Manifest.Meta.Maps.GetValueOrDefault($"map{(int)difficulty}")?.Rating ?? string.Empty))
    ];
}
