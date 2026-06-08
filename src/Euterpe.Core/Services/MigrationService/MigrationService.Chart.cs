using System.Text.Json;
using Euterpe.Core.JsonContexts;
using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;
using static Euterpe.Models.Charts.CustomAlbums.CustomAlbumFiles;

namespace Euterpe.Core;

internal sealed partial class MigrationService
{
    private async Task<MigrationOutcome> MigratePackageAsync(string packagePath, CancellationToken cancellationToken)
    {
        var folderName = Path.GetFileNameWithoutExtension(packagePath);
        var destinationFolder = Path.Combine(GameConfig.OfflineChartsFolder, folderName);

        if (Directory.Exists(destinationFolder))
        {
            Logger.ZLogInformation($"'{folderName}' already migrated, skipping");
            return MigrationOutcome.Skipped;
        }

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, folderName);
        try
        {
            await BuildEpkAsync(packagePath, workFolder, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder))
            {
                Logger.ZLogError($"Failed to publish migrated chart '{folderName}'");
                return MigrationOutcome.Failed;
            }

            Logger.ZLogInformation($"Migrated '{folderName}' -> {destinationFolder}");
            return MigrationOutcome.Migrated;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to migrate custom album '{folderName}', skipping");
            return MigrationOutcome.Failed;
        }
        finally
        {
            if (Directory.Exists(workFolder))
            {
                Directory.Delete(workFolder, true);
            }
        }
    }

    private async Task BuildEpkAsync(string packagePath, string workFolder, CancellationToken cancellationToken)
    {
        if (Directory.Exists(workFolder))
        {
            Directory.Delete(workFolder, true);
        }

        await Archive.ExtractZipFileAsync(packagePath, workFolder).ConfigureAwait(false);

        var info = await ReadInfoAsync(workFolder, cancellationToken).ConfigureAwait(false);
        File.Delete(Path.Combine(workFolder, InfoFileName));

        var backgroundVideoOpacity = await ReadCinemaOpacityAsync(workFolder, cancellationToken).ConfigureAwait(false);

        await ConvertMusicIfNeededAsync(workFolder, cancellationToken).ConfigureAwait(false);
        NormalizeVideoName(workFolder);

        var manifest = new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Cid = null,
            Meta = BuildMeta(info, BuildMaps(info), backgroundVideoOpacity),
            Files = BuildFiles(workFolder)
        };

        await WriteManifestAsync(Path.Combine(workFolder, ManifestFileName), manifest, cancellationToken).ConfigureAwait(false);
    }

    private async Task<InfoJson> ReadInfoAsync(string folder, CancellationToken cancellationToken)
    {
        var infoPath = Path.Combine(folder, InfoFileName);
        var stream = File.OpenRead(infoPath);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer.DeserializeAsync(stream, CamelCaseContext.Default.InfoJson, cancellationToken).ConfigureAwait(false)
                   ?? throw new InvalidDataException($"{InfoFileName} in '{folder}' is empty or invalid");
        }
    }

    private async Task<float?> ReadCinemaOpacityAsync(string folder, CancellationToken cancellationToken)
    {
        var cinemaPath = Path.Combine(folder, CinemaFileName);
        if (!File.Exists(cinemaPath))
        {
            return null;
        }

        Cinema cinema;
        var stream = File.OpenRead(cinemaPath);
        await using (stream.ConfigureAwait(false))
        {
            cinema = await JsonSerializer.DeserializeAsync(stream, SnakeCaseJsonContext.Default.Cinema, cancellationToken).ConfigureAwait(false)
                     ?? throw new InvalidDataException($"{CinemaFileName} in '{folder}' is empty or invalid");
        }

        File.Delete(cinemaPath);
        return cinema.Opacity;
    }

    private async Task ConvertMusicIfNeededAsync(string folder, CancellationToken cancellationToken)
    {
        var source = Directory.EnumerateFiles(folder, $"{MusicName}.*").Single();
        var extension = Path.GetExtension(source)[1..];

        switch (extension)
        {
            case "ogg":
                return;
            case "mp3":
                var target = Path.Combine(folder, MusicFileName);
                await Task.Run(() => AudioConverter.Convert(source, target, MusicExtension[1..], cancellationToken), cancellationToken).ConfigureAwait(false);
                File.Delete(source);
                break;
        }
    }

    private static void NormalizeVideoName(string folder)
    {
        var video = Directory.EnumerateFiles(folder, "*.mp4").FirstOrDefault();
        if (video is null)
        {
            return;
        }

        var target = Path.Combine(folder, VideoFileName);
        if (!video.Equals(target, StringComparison.OrdinalIgnoreCase))
        {
            File.Move(video, target, true);
        }
    }

    private static Dictionary<string, ManifestMap> BuildMaps(InfoJson info)
    {
        var maps = new Dictionary<string, ManifestMap>(StringComparer.OrdinalIgnoreCase);

        for (var index = 1; index <= 4; index++)
        {
            var (rating, designer) = ResolveDifficulty(info, index);
            if (string.IsNullOrWhiteSpace(rating))
            {
                continue;
            }

            maps[MapFileName((ChartDifficulty)index)] = new ManifestMap
            {
                Rating = rating,
                Charters = string.IsNullOrWhiteSpace(designer) ? [] : [designer],
                PredictedRating = null
            };
        }

        return maps;
    }

    private static Dictionary<string, ManifestFileEntry> BuildFiles(string folder)
    {
        var files = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in Directory.EnumerateFiles(folder))
        {
            files[Path.GetFileName(path)] = new ManifestFileEntry { Version = 1, Size = new FileInfo(path).Length };
        }

        return files;
    }

    private static ManifestMeta BuildMeta(InfoJson info, Dictionary<string, ManifestMap> maps, float? backgroundVideoOpacity)
    {
        var (bpm, bpmMin, bpmMax) = ParseBpm(info.Bpm);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new ManifestMeta
        {
            Name = info.Name,
            NameRomanized = NullIfEmpty(info.NameRomanized),
            Author = info.Author,
            Description = null,
            SafeForStreamer = false,
            Bpm = bpm,
            BpmMin = bpmMin,
            BpmMax = bpmMax,
            Scene = info.Scene,
            BackgroundVideoOpacity = backgroundVideoOpacity,
            SearchKeywords = info.SearchTags.Length == 0 ? null : info.SearchTags,
            Maps = maps,
            HideMode = NullIfEmpty(info.HideBmsMode),
            HideRatingOverride = NullIfEmpty(info.HideBmsDifficulty),
            HideMessage = NullIfEmpty(info.HideBmsMessage),
            Uploader = null,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private async Task WriteManifestAsync(string manifestPath, Manifest manifest, CancellationToken cancellationToken)
    {
        var stream = File.Create(manifestPath);
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerialization.SerializeManifestAsync(stream, manifest, cancellationToken).ConfigureAwait(false);
        }
    }

    private static (string Rating, string Designer) ResolveDifficulty(InfoJson info, int index) => index switch
    {
        1 => (info.Difficulty1, Pick(info.LevelDesigner1, info.LevelDesigner)),
        2 => (info.Difficulty2, Pick(info.LevelDesigner2, info.LevelDesigner)),
        3 => (info.Difficulty3, Pick(info.LevelDesigner3, info.LevelDesigner)),
        4 => (info.Difficulty4, Pick(info.LevelDesigner4, info.LevelDesigner)),
        _ => (string.Empty, string.Empty)
    };

    private static string Pick(string primary, string fallback) => string.IsNullOrWhiteSpace(primary) ? fallback : primary;

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static (int Bpm, int? Min, int? Max) ParseBpm(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (0, null, null);
        }

        var parts = raw.Split(['-', '~', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[^1], out var max))
        {
            return ((min + max) / 2, min, max);
        }

        return int.TryParse(parts[0], out var single) ? (single, null, null) : (0, null, null);
    }

    private void NotifyResult(int migrated, int failed)
    {
        if (migrated is 0 && failed is 0)
        {
            return;
        }

        if (failed is 0)
        {
            NotificationService.SuccessLight(Notification_Content_Migration_Success, migrated);
        }
        else
        {
            NotificationService.WarningLight(Notification_Content_Migration_Partial, migrated, failed);
        }
    }
}