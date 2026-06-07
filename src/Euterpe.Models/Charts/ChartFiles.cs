namespace Euterpe.Models.Charts;

public static class ChartFiles
{
    public const string ManifestFileName = "manifest.epk";
    public const string CoverFileName = "cover.png";
    public const string MusicFileName = "music.ogg";
    public const string VideoFileName = "video.mp4";

    public static string MapFileName(ChartDifficulty difficulty) => $"map{(int)difficulty}.bms";
}
