using System.Diagnostics;
using Euterpe.Models.Charts.CustomAlbums;
using Riok.Mapperly.Abstractions;

namespace Euterpe.Models;

public static partial class Mapper
{
    public static ManifestMeta ToManifestMeta(this InfoJson info, float? backgroundVideoOpacity, IReadOnlyList<ChartDifficulty> difficulties)
    {
        var (bpm, bpmMin, bpmMax) = ParseBpm(info.Bpm);

        return new ManifestMeta
        {
            Name = info.Name,
            NameRomanized = info.NameRomanized,
            Author = info.Author,
            Description = null,
            SafeForStreamer = info.Streamer,
            Bpm = bpm,
            BpmMin = bpmMin,
            BpmMax = bpmMax,
            Scene = NormalizeScene(info.Scene),
            BackgroundVideoOpacity = backgroundVideoOpacity,
            SearchKeywords = info.SearchTags,
            Maps = BuildMaps(info, difficulties),
            HideMode = info.HideBmsMode,
            HideRatingOverride = info.Difficulty4,
            HideMessage = info.HideBmsMessage,
            Uploader = null
        };
    }

    private static Dictionary<string, ManifestMap> BuildMaps(InfoJson info, IReadOnlyList<ChartDifficulty> difficulties)
    {
        var maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase);

        foreach (var difficulty in difficulties)
        {
            var (rating, designer) = ResolveDifficulty(info, difficulty);
            maps[ChartFiles.MapName(difficulty)] = new ManifestMap
            {
                Rating = rating,
                Charters = [designer],
                PredictedRating = null
            };
        }

        return maps;
    }

    private static (string Rating, string Designer) ResolveDifficulty(InfoJson info, ChartDifficulty difficulty) => difficulty switch
    {
        ChartDifficulty.Easy => (info.Difficulty1, info.LevelDesigner1.DefaultIfWhiteSpace(info.LevelDesigner)),
        ChartDifficulty.Hard => (info.Difficulty2, info.LevelDesigner2.DefaultIfWhiteSpace(info.LevelDesigner)),
        ChartDifficulty.Master => (info.Difficulty3, info.LevelDesigner3.DefaultIfWhiteSpace(info.LevelDesigner)),
        ChartDifficulty.Hidden => (info.Difficulty4, info.LevelDesigner4.DefaultIfWhiteSpace(info.LevelDesigner)),
        _ => throw new UnreachableException()
    };

    private static (int Bpm, int? Min, int? Max) ParseBpm(string bpmString)
    {
        if (bpmString.IsNullOrWhiteSpace())
        {
            return (0, null, null);
        }

        var parts = bpmString.Split('~', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[^1], out var max))
        {
            return ((min + max) / 2, min, max);
        }

        return int.TryParse(parts[0], out var single) ? (single, null, null) : (0, null, null);
    }

    [UserMapping(Default = false)]
    private static string NormalizeScene(string scene)
    {
        const string prefix = "scene_";
        return scene.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(scene.AsSpan(prefix.Length), out var number)
            ? $"{prefix}{number:D2}"
            : throw new InvalidDataException($"Invalid scene '{scene}'");
    }
}
