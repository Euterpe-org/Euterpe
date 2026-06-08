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

        var sources = ResolveSources(ChartLocalService.GetCustomAlbumsSourcePaths());
        int migrated = 0, skipped = 0, failed = 0;

        for (var i = 0; i < sources.Length; i++)
        {
            progress?.Report($"{sources[i].Name} ({i + 1}/{sources.Length})");

            switch (await MigrateSourceAsync(sources[i], cancellationToken).ConfigureAwait(false))
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

        if (failed > 0)
        {
            NotificationService.WarningLight(Notification_Content_Migration_Partial, migrated, failed);
            return;
        }

        FileSystemService.TryDeleteDirectory(GameConfig.CustomAlbumsChartsFolder);

        if (migrated > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Migration_Success, migrated);
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IArchiveService Archive { get; init; }
    public required IAudioConverterService AudioConverter { get; init; }
    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IJsonSerializationService JsonSerialization { get; init; }
    public required ILogger<MigrationService> Logger { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}