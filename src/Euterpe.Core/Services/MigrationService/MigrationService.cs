using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class MigrationService : IMigrationService
{
    public async Task<(MigrationOutcome Outcome, string Destination)> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default)
    {
        var name = source.Name;
        var desiredFolder = Path.Combine(GameConfig.OfflineChartsFolder, name);
        var workFolder = Path.Combine(GameConfig.TempChartsFolder, Guid.NewGuid().ToString("N"));

        try
        {
            await PopulateWorkFolderAsync(source, workFolder).ConfigureAwait(false);

            if (!HasSupportedMusic(workFolder))
            {
                Logger.ZLogInformation($"'{name}' uses an unsupported audio format, skipping migration");
                return (MigrationOutcome.Unsupported, desiredFolder);
            }

            await BuildChartAsync(workFolder, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectoryToAvailablePath(workFolder, desiredFolder, out var destinationFolder))
            {
                Logger.ZLogError($"Failed to move migrated chart '{name}'");
                return (MigrationOutcome.Failed, desiredFolder);
            }

            Logger.ZLogInformation($"Migrated '{name}' -> {destinationFolder}");
            return (MigrationOutcome.Migrated, destinationFolder);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to migrate custom album '{name}', skipping");
            return (MigrationOutcome.Failed, desiredFolder);
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
