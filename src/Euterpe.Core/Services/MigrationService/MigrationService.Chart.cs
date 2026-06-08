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
            await BuildEpkAsync(source, workFolder, cancellationToken).ConfigureAwait(false);

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
            if (Directory.Exists(workFolder))
            {
                Directory.Delete(workFolder, true);
            }
        }
    }

    private static CustomAlbumSource[] ResolveSources(IEnumerable<string> paths) =>
        paths
            .Select(ToSource)
            .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.MaxBy(SourceTimestamp)!)
            .ToArray();

    private static CustomAlbumSource ToSource(string path)
    {
        var isFolder = Directory.Exists(path);
        return new CustomAlbumSource(
            path,
            isFolder ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path),
            isFolder);
    }

    private static DateTime SourceTimestamp(CustomAlbumSource source) =>
        source.IsFolder
            ? Directory.EnumerateFiles(source.Path).Max(File.GetLastWriteTimeUtc)
            : File.GetLastWriteTimeUtc(source.Path);
}