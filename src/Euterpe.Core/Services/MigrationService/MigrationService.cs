namespace Euterpe.Core;

internal sealed partial class MigrationService : IMigrationService
{
    public async Task MigrateCustomAlbumsAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(GameConfig.CustomAlbumsChartsFolder))
        {
            Logger.ZLogInformation($"No CustomAlbums folder at {GameConfig.CustomAlbumsChartsFolder}, nothing to migrate");
            return;
        }

        var packages = ChartLocalService.GetCustomAlbumsChartFilePaths();
        int migrated = 0, skipped = 0, failed = 0;

        for (var i = 0; i < packages.Length; i++)
        {
            progress?.Report($"{Path.GetFileNameWithoutExtension(packages[i])} ({i + 1}/{packages.Length})");

            switch (await MigratePackageAsync(packages[i], cancellationToken).ConfigureAwait(false))
            {
                case MigrationOutcome.Migrated:
                    migrated++;
                    break;
                case MigrationOutcome.Skipped:
                    skipped++;
                    break;
                case MigrationOutcome.Failed:
                    failed++;
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        Logger.ZLogInformation($"CustomAlbums migration complete: {migrated} migrated, {skipped} skipped, {failed} failed");
        NotifyResult(migrated, failed);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IArchiveService Archive { get; init; }
    public required IAudioConverterService AudioConverter { get; init; }
    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<MigrationService> Logger { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}