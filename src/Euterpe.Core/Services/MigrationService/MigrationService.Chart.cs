using Euterpe.Core.JsonContexts;
using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;
using static Euterpe.Models.Charts.CustomAlbums.CustomAlbumFiles;

namespace Euterpe.Core;

internal sealed partial class MigrationService
{
    private async Task<MigrationOutcome> MigrateSourceAsync(CustomAlbumSource source, CancellationToken cancellationToken)
    {
        var name = source.Name;
        var destinationFolder = Path.Combine(GameConfig.OfflineChartsFolder, name);

        if (Directory.Exists(destinationFolder))
        {
            Logger.ZLogInformation($"'{name}' already migrated, skipping");
            return MigrationOutcome.Skipped;
        }

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, name);
        try
        {
            await BuildChartAsync(source, workFolder, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder))
            {
                Logger.ZLogError($"Failed to move migrated chart '{name}'");
                return MigrationOutcome.Failed;
            }

            Logger.ZLogInformation($"Migrated '{name}' -> {destinationFolder}");
            return MigrationOutcome.Migrated;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to migrate custom album '{name}', skipping");
            return MigrationOutcome.Failed;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    private async Task BuildChartAsync(CustomAlbumSource source, string workFolder, CancellationToken cancellationToken)
    {
        FileSystemService.DeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);

        await PopulateWorkFolderAsync(source, workFolder).ConfigureAwait(false);

        var info = await ReadInfoAsync(workFolder, cancellationToken).ConfigureAwait(false);
        var cinema = await ReadCinemaAsync(workFolder, cancellationToken).ConfigureAwait(false);

        await NormalizeAudioAsync(workFolder, MusicName, true, cancellationToken).ConfigureAwait(false);
        await NormalizeAudioAsync(workFolder, DemoName, false, cancellationToken).ConfigureAwait(false);
        NormalizeVideoName(workFolder);
        DeleteConsumedSources(workFolder);

        await WriteManifestAsync(workFolder, info, cinema, cancellationToken).ConfigureAwait(false);
    }

    private async Task PopulateWorkFolderAsync(CustomAlbumSource source, string workFolder)
    {
        if (!source.IsFolder)
        {
            await Archive.ExtractZipFileAsync(source.Path, workFolder).ConfigureAwait(false);
            return;
        }

        try
        {
            FileSystemService.CopyDirectory(source.Path, workFolder);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to copy folder chart '{source.Path}'", ex);
        }
    }

    private async ValueTask<InfoJson> ReadInfoAsync(string folder, CancellationToken cancellationToken)
    {
        var infoPath = Path.Combine(folder, InfoFileName);
        return File.Exists(infoPath)
            ? await JsonSerialization.DeserializeFromFileAsync(infoPath, CamelCaseContext.Default.InfoJson, cancellationToken).ConfigureAwait(false)
            : throw new FileNotFoundException($"{InfoFileName} is missing", infoPath);
    }

    private async ValueTask<Cinema?> ReadCinemaAsync(string folder, CancellationToken cancellationToken)
    {
        var cinemaPath = Path.Combine(folder, CinemaFileName);
        return File.Exists(cinemaPath)
            ? await JsonSerialization.DeserializeFromFileAsync(cinemaPath, SnakeCaseJsonContext.Default.Cinema, cancellationToken).ConfigureAwait(false)
            : null;
    }

    private async Task NormalizeAudioAsync(string folder, string name, bool required, CancellationToken cancellationToken)
    {
        var matches = Directory.EnumerateFiles(folder, $"{name}.*");
        var source = required ? matches.Single() : matches.SingleOrDefault();
        if (source is null)
        {
            return;
        }

        switch (Path.GetExtension(source)[1..])
        {
            case "ogg":
                return;
            case "mp3":
                var target = Path.Combine(folder, $"{name}{MusicExtension}");
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
        if (string.Equals(target, video, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        File.Move(video, target, true);
    }

    private void DeleteConsumedSources(string folder)
    {
        FileSystemService.TryDeleteFile(Path.Combine(folder, InfoFileName));
        FileSystemService.TryDeleteFile(Path.Combine(folder, CinemaFileName), DeleteOption.IgnoreIfNotFound);
    }
}