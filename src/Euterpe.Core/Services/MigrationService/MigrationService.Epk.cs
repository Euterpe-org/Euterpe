using Euterpe.Core.JsonContexts;
using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;
using static Euterpe.Models.Charts.CustomAlbums.CustomAlbumFiles;

namespace Euterpe.Core;

internal sealed partial class MigrationService
{
    private async Task BuildEpkAsync(CustomAlbumSource source, string workFolder, CancellationToken cancellationToken)
    {
        if (Directory.Exists(workFolder))
        {
            Directory.Delete(workFolder, true);
        }

        await PopulateWorkFolderAsync(source, workFolder).ConfigureAwait(false);

        var info = await ReadInfoAsync(workFolder, cancellationToken).ConfigureAwait(false);
        var cinema = await ReadCinemaAsync(workFolder, cancellationToken).ConfigureAwait(false);

        await NormalizeMusicAsync(workFolder, cancellationToken).ConfigureAwait(false);
        NormalizeVideoName(workFolder);
        DeleteConsumedSources(workFolder);

        var manifest = new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Cid = null,
            Meta = CustomAlbumMapper.ToManifestMeta(info, cinema?.Opacity),
            Files = BuildFiles(workFolder)
        };

        var manifestPath = Path.Combine(workFolder, ManifestFileName);
        await WriteManifestAsync(manifestPath, manifest, cancellationToken).ConfigureAwait(false);
    }

    // .mdm packages are zip archives; folder charts are copied verbatim. Both land in the same work folder.
    private async Task PopulateWorkFolderAsync(CustomAlbumSource source, string workFolder)
    {
        if (!source.IsFolder)
        {
            await Archive.ExtractZipFileAsync(source.Path, workFolder).ConfigureAwait(false);
            return;
        }

        if (!FileSystemService.TryCopyDirectory(source.Path, workFolder))
        {
            throw new IOException($"Failed to copy folder chart '{source.Path}'");
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

    private async Task NormalizeMusicAsync(string folder, CancellationToken cancellationToken)
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
        if (string.Equals(target, video, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        File.Move(video, target, true);
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

    private async Task WriteManifestAsync(string manifestPath, Manifest manifest, CancellationToken cancellationToken)
    {
        var stream = File.Create(manifestPath);
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerialization.SerializeManifestAsync(stream, manifest, cancellationToken).ConfigureAwait(false);
        }
    }

    private void DeleteConsumedSources(string folder)
    {
        FileSystemService.TryDeleteFile(Path.Combine(folder, InfoFileName));
        FileSystemService.TryDeleteFile(Path.Combine(folder, CinemaFileName), DeleteOption.IgnoreIfNotFound);
    }
}