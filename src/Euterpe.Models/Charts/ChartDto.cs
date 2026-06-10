using System.Globalization;
using Euterpe.Localization;
using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Models.Charts;

public sealed partial class ChartDto : ObservableObject
{
    public required string FolderPath { get; init; }
    public required string FolderName { get; init; }
    public required Manifest Manifest { get; init; }
    public ChartSource Source { get; init; } = ChartSource.Offline;

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    public bool IsOnline => Source is ChartSource.Online;

    public string? DetailUrl =>
        Manifest.Cid is { } cid ? $"{EuterpeWeb.BaseUrl}/charts/{cid}" : null;

    public string? UploaderPageUrl =>
        Manifest.Meta.Uploader is { } uploader ? $"{EuterpeWeb.BaseUrl}/users/{uploader.Uid}?tab=charts" : null;

    public string? UploaderDisplay =>
        Manifest.Meta.Uploader is { } uploader
            ? string.Format(CultureInfo.CurrentCulture, XAML.ChartManage_UploadedBy, uploader.Nickname)
            : null;

    public string? CoverPath => CoverExtensions
        .Select(extension => AssetPath(CoverName + extension))
        .FirstOrDefault(path => path is not null);

    public string? MusicPath => AssetPath(MusicFileName);
    public string? DemoPath => AssetPath(DemoFileName);
    public string? VideoPath => AssetPath(VideoFileName);

    public IReadOnlyList<ChartDifficulty> Difficulties =>
        Manifest.Files.ExistingDifficulties();

    public IReadOnlyList<DifficultyBadge> DifficultyBadges =>
    [
        .. Difficulties.Select(difficulty =>
            new DifficultyBadge(difficulty, Manifest.Meta.Maps.GetValueOrDefault(MapName(difficulty))?.Rating ?? string.Empty))
    ];

    public double MaxRating =>
        Manifest.Meta.Maps.Values.Select(static map => map.RatingValue).DefaultIfEmpty(-1).Max();

    public long SizeBytes => Manifest.Files.Values.Sum(file => file.Size);

    public string SizeDisplay => SizeBytes switch
    {
        var bytes and >= 1 << 30 => string.Create(CultureInfo.InvariantCulture, $"{bytes / (double)(1 << 30):0.#} GB"),
        var bytes and >= 1 << 20 => string.Create(CultureInfo.InvariantCulture, $"{bytes / (double)(1 << 20):0.#} MB"),
        var bytes => string.Create(CultureInfo.InvariantCulture, $"{bytes / (double)(1 << 10):0.#} KB")
    };

    public string BpmDisplay =>
        Manifest.Meta is { BpmMin: { } min, BpmMax: { } max } && min != max
            ? $"{min}–{max}"
            : Manifest.Meta.Bpm.ToString(CultureInfo.InvariantCulture);

    public string CharterDisplay =>
        string.Join(", ", Manifest.Meta.Maps.Values
            .SelectMany(map => map.Charters)
            .Distinct(StringComparer.OrdinalIgnoreCase));

    public bool HasDifficulty(ChartDifficulty difficulty) =>
        Manifest.Files.ContainsKey(MapFileName(difficulty));

    public string? BmsPath(ChartDifficulty difficulty) =>
        AssetPath(MapFileName(difficulty));

    private string? AssetPath(string fileName) =>
        Manifest.Files.ContainsKey(fileName) ? Path.Combine(FolderPath, fileName) : null;
}
