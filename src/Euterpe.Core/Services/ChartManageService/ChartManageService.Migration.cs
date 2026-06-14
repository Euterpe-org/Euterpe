using System.Collections.Concurrent;
using Avalonia.Threading;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task<int> MigrateCustomAlbumsAsync(IProgress<MigrationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(GameConfig.CustomAlbumsChartsFolder))
        {
            Logger.ZLogInformation($"No CustomAlbums folder at {GameConfig.CustomAlbumsChartsFolder}, nothing to migrate");
            return 0;
        }

        var sources = ChartLocalService.GetCustomAlbumsSources();
        var migratedCharts = new ConcurrentBag<ChartDto>();
        var outcomes = new ConcurrentBag<MigrationOutcome>();
        var completed = 0;

        await Parallel.ForEachAsync(
                sources,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken },
                async (source, token) =>
                {
                    outcomes.Add(await MigrateSourceAsync(source, migratedCharts, token).ConfigureAwait(false));
                    progress?.Report(new MigrationProgress(Interlocked.Increment(ref completed), sources.Length));
                })
            .ConfigureAwait(false);

        if (!migratedCharts.IsEmpty)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _sourceCache.AddOrUpdate(migratedCharts)).GetTask().ConfigureAwait(false);
        }

        var migrated = outcomes.Count(outcome => outcome is MigrationOutcome.Migrated);
        var skipped = outcomes.Count(outcome => outcome is MigrationOutcome.Skipped);
        var failed = outcomes.Count(outcome => outcome is MigrationOutcome.Failed);
        Logger.ZLogInformation($"CustomAlbums migration complete: {migrated} migrated, {skipped} skipped, {failed} failed");

        if (!Directory.EnumerateFileSystemEntries(GameConfig.CustomAlbumsChartsFolder).Any())
        {
            FileSystemService.TryDeleteDirectory(GameConfig.CustomAlbumsChartsFolder, DeleteOption.IgnoreIfNotFound);
        }

        if (failed > 0)
        {
            NotificationService.WarningLight(Notification_Content_Migration_Partial, migrated, failed);
            return migrated + failed;
        }

        if (migrated > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Migration_Success, migrated);
        }

        return migrated;
    }

    private async Task<MigrationOutcome> MigrateSourceAsync(CustomAlbumSource source, ConcurrentBag<ChartDto> migratedCharts, CancellationToken cancellationToken)
    {
        var (outcome, destination) = await MigrationService.MigrateCustomAlbumAsync(source, cancellationToken).ConfigureAwait(false);
        switch (outcome)
        {
            case MigrationOutcome.Migrated:
                DeleteSource(source);
                if (await ChartLocalService.LoadChartFromPathAsync(destination, ChartSource.Offline).ConfigureAwait(false) is { } chart)
                {
                    migratedCharts.Add(chart);
                }
                else
                {
                    Logger.ZLogWarning($"Migrated chart at {destination} could not be loaded into the cache");
                }

                break;
            case MigrationOutcome.Skipped:
                DeleteSource(source);
                break;
        }

        return outcome;
    }

    private void DeleteSource(CustomAlbumSource source)
    {
        if (source.IsFolder)
        {
            FileSystemService.TryDeleteDirectory(source.Path, DeleteOption.IgnoreIfNotFound);
        }
        else
        {
            FileSystemService.TryDeleteFile(source.Path, DeleteOption.IgnoreIfNotFound);
        }
    }
}
