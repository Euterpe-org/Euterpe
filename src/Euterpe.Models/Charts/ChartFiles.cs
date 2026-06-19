namespace Euterpe.Models.Charts;

public static class ChartFiles
{
    public const string ManifestExtension = ".epk";
    public const string ManifestFileName = $"manifest{ManifestExtension}";
    public const string PackageExtension = ".zip";
    public const string MusicFileName = $"{MusicName}{MusicExtension}";
    public const string DemoFileName = $"{DemoName}{MusicExtension}";
    public const string VideoFileName = "video.mp4";

    public const string CoverName = "cover";

    public const string MusicName = "music";
    public const string DemoName = "demo";
    public const string MusicExtension = ".ogg";

    public static readonly IReadOnlyList<string> CoverExtensions = [".webp", ".png", ".gif"];

    public static bool IsLargeMedia(string fileName) => fileName == VideoFileName;

    public static string MapName(ChartDifficulty difficulty) => $"map{(int)difficulty}";

    public static string MapFileName(ChartDifficulty difficulty) => $"map{(int)difficulty}.bms";

    public static IReadOnlyList<ChartDifficulty> ExistingDifficulties(this IReadOnlyDictionary<string, ManifestFileEntry> files) =>
        [.. ChartDifficultyExtensions.GetValues().Where(difficulty => files.ContainsKey(MapFileName(difficulty)))];
}
