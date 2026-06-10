using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task<bool> ImportChartsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default)
    {
        var imported = 0;

        foreach (var path in paths)
        {
            if (await ImportChartAsync(path, cancellationToken).ConfigureAwait(false))
            {
                imported++;
            }
        }

        return imported > 0;
    }

    private async Task<bool> ImportChartAsync(string path, CancellationToken cancellationToken)
    {
        var name = Directory.Exists(path) ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        var kind = ClassifyChartDrop(path);

        if (kind is ChartDropKind.Unsupported)
        {
            Logger.ZLogWarning($"Ignored dropped path (not a chart package or folder): {path}");
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Unsupported, name);
            return false;
        }

        try
        {
            var (outcome, destination) = kind is ChartDropKind.Package
                ? await ExtractPackagedChartAsync(path, name).ConfigureAwait(false)
                : await MigrationService.MigrateCustomAlbumAsync(ChartLocalService.CreateCustomAlbumSource(path), cancellationToken).ConfigureAwait(false);

            if (outcome is MigrationOutcome.Migrated)
            {
                await AddImportedChartAsync(destination).ConfigureAwait(false);
            }

            return ReportOutcome(outcome, name, path);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to import chart from {path}");
            NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed, name);
            return false;
        }
    }

    private async Task AddImportedChartAsync(string chartFolder)
    {
        if (await ChartLocalService.LoadChartFromPathAsync(chartFolder, ChartSource.Offline).ConfigureAwait(false) is { } chart)
        {
            _sourceCache.AddOrUpdate(chart);
        }
        else
        {
            Logger.ZLogWarning($"Imported chart at {chartFolder} could not be loaded into the cache");
        }
    }

    private static ChartDropKind ClassifyChartDrop(string path)
    {
        if (Directory.Exists(path))
        {
            return File.Exists(Path.Combine(path, CustomAlbumFiles.InfoFileName))
                ? ChartDropKind.CustomAlbum
                : ChartDropKind.Unsupported;
        }

        if (!File.Exists(path))
        {
            return ChartDropKind.Unsupported;
        }

        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            CustomAlbumFiles.PackageExtension => ChartDropKind.CustomAlbum,
            ChartFiles.PackageExtension => ChartDropKind.Package,
            _ => ChartDropKind.Unsupported
        };
    }

    private async Task<(MigrationOutcome Outcome, string Destination)> ExtractPackagedChartAsync(string path, string name)
    {
        var destination = Path.Combine(GameConfig.OfflineChartsFolder, name);
        if (Directory.Exists(destination))
        {
            return (MigrationOutcome.Skipped, destination);
        }

        try
        {
            await Archive.ExtractZipFileAsync(path, destination).ConfigureAwait(false);
            return (MigrationOutcome.Migrated, destination);
        }
        catch
        {
            FileSystemService.TryDeleteDirectory(destination, DeleteOption.IgnoreIfNotFound);
            throw;
        }
    }

    private bool ReportOutcome(MigrationOutcome outcome, string name, string path)
    {
        switch (outcome)
        {
            case MigrationOutcome.Migrated:
                Logger.ZLogInformation($"Imported chart {name} from {path}");
                NotificationService.SuccessLight(Notification_Content_Chart_Import_Success, name);
                return true;
            case MigrationOutcome.Skipped:
                Logger.ZLogInformation($"Import of {name} skipped: chart already exists");
                NotificationService.WarningLight(Notification_Content_Chart_Import_Duplicated, name);
                return false;
            case MigrationOutcome.Failed:
                NotificationService.ErrorLight(Notification_Content_Chart_Import_Failed, name);
                return false;
            default:
                throw new UnreachableException();
        }
    }

    private enum ChartDropKind
    {
        CustomAlbum,
        Package,
        Unsupported
    }
}
