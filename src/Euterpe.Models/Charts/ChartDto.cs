using System.Globalization;
using Euterpe.Localization;
using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Models.Charts;

public sealed partial class ChartDto : ObservableObject
{
    private double? _maxRating;
    private long? _sizeBytes;

    // Chart Properties
    public required string FolderPath { get; init; }
    public required string FolderName { get; init; }
    public required Manifest Manifest { get; init; }
    public ChartSource Source { get; init; } = ChartSource.Offline;

    // Binding Boolean Properties
    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    public bool IsOnline => Source is ChartSource.Online;

    // Web Urls
    public string? DetailUrl =>
        Manifest.Cid is { } cid ? $"{EuterpeWeb.BaseUrl}/charts/{cid}" : null;

    public string? UploaderPageUrl =>
        Manifest.Meta.Uploader is { } uploader ? $"{EuterpeWeb.BaseUrl}/users/{uploader.Uid}?tab=charts" : null;

    // Asset Paths
    public string? CoverPath => CoverExtensions
        .Select(extension => AssetPath(CoverName + extension))
        .FirstOrDefault(path => path is not null);

    public string? AudioPath => AssetPath(DemoFileName) ?? AssetPath(MusicFileName);
    public string? VideoPath => AssetPath(VideoFileName);

    // Difficulties
    public IReadOnlyList<ChartDifficulty> Difficulties =>
        field ??= Manifest.Files.ExistingDifficulties();

    public IReadOnlyList<DifficultyBadge> DifficultyBadges =>
        field ??= [.. Difficulties.Select(difficulty => new DifficultyBadge(difficulty, Manifest.Meta.Maps[MapName(difficulty)].Rating))];

    // Sort Values
    public double MaxRating =>
        _maxRating ??= Manifest.Meta.Maps.Values.Max(static map => map.RatingValue);

    public long SizeBytes => _sizeBytes ??= Manifest.Files.Values.Sum(static file => file.Size);

    // Display Strings
    public string? UploaderDisplay =>
        Manifest.Meta.Uploader is { } uploader
            ? string.Format(XAML.ChartManage_UploadedBy, uploader.Nickname)
            : null;

    public string SizeDisplay
    {
        get
        {
            var (divisor, unit) = SizeBytes switch
            {
                >= 1 << 30 => (1 << 30, "GB"),
                >= 1 << 20 => (1 << 20, "MB"),
                _ => (1 << 10, "KB")
            };
            return string.Create(CultureInfo.InvariantCulture, $"{SizeBytes / (double)divisor:0.#} {unit}");
        }
    }

    public string BpmDisplay =>
        Manifest.Meta is { BpmMin: { } min, BpmMax: { } max }
            ? $"{min}–{max}"
            : Manifest.Meta.Bpm.ToString();

    public string CharterDisplay =>
        string.Join(", ", Manifest.Meta.Maps.Values
            .SelectMany(map => map.Charters)
            .Distinct(StringComparer.OrdinalIgnoreCase));

    // Chart Files
    public bool HasDifficulty(ChartDifficulty difficulty) => Difficulties.Contains(difficulty);

    private string? AssetPath(string fileName) =>
        Manifest.Files.ContainsKey(fileName) ? Path.Combine(FolderPath, fileName) : null;
}
