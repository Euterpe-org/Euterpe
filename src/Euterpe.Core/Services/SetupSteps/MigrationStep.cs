namespace Euterpe.Core;

internal sealed class MigrationStep : ISetupStep
{
    #region Injections

    public required IChartManageService ChartManageService { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.Migration;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        progress?.Report("Migrating CustomAlbums Charts ...");
        await ChartManageService.MigrateCustomAlbumsAsync(progress, cancellationToken).ConfigureAwait(false);
    }
}
