using Euterpe.Models.Charts.CustomAlbums;
using static Euterpe.Models.Charts.ChartFiles;

namespace Euterpe.Core;

internal sealed partial class MigrationService
{
    private async Task WriteManifestAsync(string folder, InfoJson info, Cinema? cinema, CancellationToken cancellationToken)
    {
        var files = BuildFiles(folder);
        var difficulties = files.ExistingDifficulties();

        var manifest = new Manifest
        {
            Schema = Manifest.CurrentSchema,
            Cid = null,
            Meta = CustomAlbumMapper.ToManifestMeta(info, cinema?.Opacity, difficulties),
            Files = files
        };

        var manifestPath = Path.Combine(folder, ManifestFileName);
        var stream = File.Create(manifestPath);
        await using (stream.ConfigureAwait(false))
        {
            await MessagePackSerialization.SerializeManifestAsync(stream, manifest, cancellationToken).ConfigureAwait(false);
        }
    }

    private static Dictionary<string, ManifestFileEntry> BuildFiles(string folder)
    {
        var files = new Dictionary<string, ManifestFileEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in new DirectoryInfo(folder).EnumerateFiles())
        {
            files[file.Name] = new ManifestFileEntry { Version = 1, Size = file.Length };
        }

        return files;
    }
}
