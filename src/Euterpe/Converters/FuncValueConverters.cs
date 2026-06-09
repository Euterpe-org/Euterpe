using System.Collections.Concurrent;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Euterpe.Converters;

public static class FuncValueConverters
{
    private static readonly ConcurrentDictionary<ChartDifficulty, Bitmap> DifficultyIcons = new();

    public static FuncValueConverter<string, StreamGeometry?> SemiIconConverter { get; } =
        new(key => GetCurrentApplication().TryGetResource($"SemiIcon{key}", out var result) ? result as StreamGeometry : null);

    public static FuncValueConverter<int, HorizontalAlignment> HorizontalAlignmentConverter { get; } =
        new(count => count is 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left);

    public static FuncValueConverter<string?, IBrush?> HexColorToBrushConverter { get; } =
        new(hex => Color.TryParse(hex, out var color) ? new SolidColorBrush(color) : null);

    public static FuncValueConverter<ChartDifficulty, Bitmap> DifficultyIconConverter { get; } =
        new(static difficulty => DifficultyIcons.GetOrAdd(difficulty, static d =>
            new Bitmap(AssetLoader.Open(AppAssets.Uri($"Difficulties/{d}.png")))));

    public static FuncValueConverter<ManifestMeta, string> ChartBpmConverter { get; } =
        new(static meta => meta is null ? string.Empty : ChartRating.BpmDisplay(meta));

    public static FuncValueConverter<bool, string> SortDirectionConverter { get; } =
        new(static descending => descending ? "↓" : "↑");

    public static FuncValueConverter<ChartDto, IReadOnlyList<DifficultyBadge>> ChartDifficultyBadgesConverter { get; } =
        new(static chart => chart is null ? [] : ChartRating.Badges(chart));

    // [folderName, currentlyPlaying] -> is this chart the one playing
    public static FuncMultiValueConverter<string?, bool> ChartIsPlayingConverter { get; } =
        new(static values => values.ToArray() is [{ } folder, var playing] && folder == playing);

    // [folderName, currentlyPlaying] -> stop glyph when playing, else play glyph
    public static FuncMultiValueConverter<string?, string> ChartPlayGlyphConverter { get; } =
        new(static values => values.ToArray() is [{ } folder, var playing] && folder == playing ? "⏹" : "▶");
}
