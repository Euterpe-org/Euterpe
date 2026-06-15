using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class MigrationService : IMigrationService
{
    public async Task<(MigrationOutcome Outcome, string Destination)> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default)
    {
        var name = source.Name;
        var destinationFolder = Path.Combine(GameConfig.OfflineChartsFolder, name);

        if (Directory.Exists(destinationFolder))
        {
            Logger.ZLogInformation($"'{name}' already migrated, skipping");
            return (MigrationOutcome.Skipped, destinationFolder);
        }

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, name);
        try
        {
            FileSystemService.DeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
            await PopulateWorkFolderAsync(source, workFolder).ConfigureAwait(false);

            if (!HasSupportedAudio(workFolder))
            {
                Logger.ZLogInformation($"'{name}' uses an unsupported audio format, skipping migration");
                return (MigrationOutcome.Unsupported, destinationFolder);
            }

            await BuildChartAsync(workFolder, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder))
            {
                Logger.ZLogError($"Failed to move migrated chart '{name}'");
                return (MigrationOutcome.Failed, destinationFolder);
            }

            Logger.ZLogInformation($"Migrated '{name}' -> {destinationFolder}");
            return (MigrationOutcome.Migrated, destinationFolder);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to migrate custom album '{name}', skipping");
            return (MigrationOutcome.Failed, destinationFolder);
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IArchiveService Archive { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IJsonSerializationService JsonSerialization { get; init; }
    public required ILogger<MigrationService> Logger { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }

    #endregion Injections
}
