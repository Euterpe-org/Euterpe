using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Models.Charts;

public sealed class ChartDto : ObservableObject
{
    public required string FolderPath { get; init; }
    public required string FolderName { get; init; }
    public required Manifest Manifest { get; init; }
    public ChartSource Source { get; init; } = ChartSource.Offline;

    public string? CoverPath => CoverExtensions
        .Select(extension => AssetPath(CoverName + extension))
        .FirstOrDefault(path => path is not null);

    public string? MusicPath => AssetPath(MusicFileName);
    public string? VideoPath => AssetPath(VideoFileName);

    public IReadOnlyList<ChartDifficulty> Difficulties =>
        Manifest.Files.ExistingDifficulties();

    public bool HasDifficulty(ChartDifficulty difficulty) =>
        Manifest.Files.ContainsKey(MapFileName(difficulty));

    public string? BmsPath(ChartDifficulty difficulty) =>
        AssetPath(MapFileName(difficulty));

    private string? AssetPath(string fileName) =>
        Manifest.Files.ContainsKey(fileName) ? Path.Combine(FolderPath, fileName) : null;
}