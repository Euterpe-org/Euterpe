using System.Collections.Concurrent;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Euterpe.Converters;

public static class FuncValueConverters
{
    private static readonly ConcurrentDictionary<ChartDifficulty, Bitmap> DifficultyIcons = new();

    // Real vector icons (not glyphs): Play = right-pointing triangle; Pause = two bars.
    private static readonly Geometry PlayIcon = StreamGeometry.Parse("M0,0 L0,12 L10,6 Z");
    private static readonly Geometry PauseIcon = StreamGeometry.Parse("M0,0 L3,0 L3,12 L0,12 Z M6,0 L9,0 L9,12 L6,12 Z");

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

    // [folderName, currentlyPlaying] -> pause icon when this chart is playing, else play icon
    public static FuncMultiValueConverter<string?, Geometry> ChartPlayIconConverter { get; } =
        new(static values => values.ToArray() is [{ } folder, var playing] && folder == playing ? PauseIcon : PlayIcon);

    // [folderName, currentlyPlaying, isCoverHovered] -> cover overlay opacity.
    // The mask + play/pause button show whenever the chart is NOT playing, or the cover is hovered;
    // they hide (revealing the cover) only while playing and not hovered.
    public static FuncMultiValueConverter<object?, double> ChartOverlayOpacityConverter { get; } =
        new(static values =>
            values.ToArray() is [var folder, var playing, var hover] && Equals(folder, playing) && hover is not true
                ? 0d
                : 1d);
}
