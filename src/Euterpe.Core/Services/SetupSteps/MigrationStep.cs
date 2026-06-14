namespace Euterpe.Core;

internal sealed class MigrationStep : ISetupStep
{
    #region Injections

    public required IChartManageService ChartManageService { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.Migration;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress.Report("Migrating CustomAlbums Charts ...");

        var migrationProgress = new Progress<MigrationProgress>(p => progress.Report($"{p.Completed}/{p.Total}"));

        await ChartManageService.MigrateCustomAlbumsAsync(migrationProgress, cancellationToken).ConfigureAwait(false);
    }
}
