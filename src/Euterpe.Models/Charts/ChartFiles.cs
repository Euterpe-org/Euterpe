namespace Euterpe.Models.Charts;

public static class ChartFiles
{
    public const string ManifestFileName = "manifest.epk";
    public const string MusicFileName = $"{MusicName}{MusicExtension}";
    public const string VideoFileName = "video.mp4";

    public const string CoverName = "cover";

    public const string MusicName = "music";
    public const string MusicExtension = ".ogg";

    public static readonly IReadOnlyList<string> CoverExtensions = [".png", ".gif"];

    private static readonly ChartDifficulty[] AllDifficulties =
    [
        ChartDifficulty.Easy,
        ChartDifficulty.Hard,
        ChartDifficulty.Master,
        ChartDifficulty.Hidden
    ];

    public static string MapFileName(ChartDifficulty difficulty) => $"map{(int)difficulty}.bms";

    public static IReadOnlyList<ChartDifficulty> ExistingDifficulties(this IReadOnlyDictionary<string, ManifestFileEntry> files) =>
        [.. AllDifficulties.Where(difficulty => files.ContainsKey(MapFileName(difficulty)))];
}