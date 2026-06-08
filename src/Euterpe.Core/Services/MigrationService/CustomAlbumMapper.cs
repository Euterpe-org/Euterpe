using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal static class CustomAlbumMapper
{
    public static ManifestMeta ToManifestMeta(InfoJson info, float? backgroundVideoOpacity)
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
            Scene = info.Scene,
            BackgroundVideoOpacity = backgroundVideoOpacity,
            SearchKeywords = info.SearchTags,
            Maps = BuildMaps(info),
            HideMode = info.HideBmsMode,
            HideRatingOverride = info.Difficulty4,
            HideMessage = info.HideBmsMessage,
            Uploader = null
        };
    }

    private static Dictionary<string, ManifestMap> BuildMaps(InfoJson info)
    {
        var maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase);

        for (var index = 1; index <= 4; index++)
        {
            var (rating, designer) = ResolveDifficulty(info, index);
            if (rating.IsNullOrWhiteSpace())
            {
                continue;
            }

            maps[$"map{index}"] = new ManifestMap
            {
                Rating = rating,
                Charters = [designer],
                PredictedRating = null
            };
        }

        return maps;
    }

    private static (string Rating, string Designer) ResolveDifficulty(InfoJson info, int index) => index switch
    {
        1 => (info.Difficulty1, info.LevelDesigner1.DefaultIfWhiteSpace(info.LevelDesigner)),
        2 => (info.Difficulty2, info.LevelDesigner2.DefaultIfWhiteSpace(info.LevelDesigner)),
        3 => (info.Difficulty3, info.LevelDesigner3.DefaultIfWhiteSpace(info.LevelDesigner)),
        4 => (info.Difficulty4, info.LevelDesigner4.DefaultIfWhiteSpace(info.LevelDesigner)),
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
}