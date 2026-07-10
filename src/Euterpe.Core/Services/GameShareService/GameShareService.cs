namespace Euterpe.Core;

internal sealed partial class GameShareService : IGameShareService
{
    public string CreateChartShareLink(IReadOnlyCollection<int> chartIds)
    {
        ArgumentNullException.ThrowIfNull(chartIds);
        var distinctChartIds = chartIds.Distinct().ToArray();
        if (distinctChartIds is []
            || distinctChartIds.Length > GameSharePackage.MaximumChartCount
            || distinctChartIds.Any(static chartId => chartId <= 0))
        {
            throw new ArgumentException(
                $"Chart IDs must contain between 1 and {GameSharePackage.MaximumChartCount} positive values.", nameof(chartIds));
        }

        return CreateShareLink(new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameConfig.Id,
            ChartIds = distinctChartIds
        });
    }

    public async Task<string?> CreateInstalledModsShareLinkAsync()
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);
        var mods = ModManageService.GetInstalledMods()
            .Where(static mod => mod.HasDownloadSource)
            .Select(static mod => new GameShareMod { Name = mod.Name, IsDisabled = mod.IsDisabled })
            .ToArray();
        if (mods is [])
        {
            return null;
        }

        return CreateShareLink(new GameSharePackage
        {
            SchemaVersion = GameSharePackage.CurrentSchemaVersion,
            GameId = GameConfig.Id,
            Mods = mods
        });
    }

    public GameSharePackage? TryParseShareLink(string text)
    {
        if (ExtractShareCode(text) is not { } code)
        {
            return null;
        }

        try
        {
            var package = MessagePackSerialization.DeserializeGameSharePackage(code.FromBase64Url());
            return IsValidPackage(package) ? package : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GameShareImportResult> ImportAsync(GameSharePackage package, IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (!IsValidPackage(package))
        {
            throw new ArgumentException("The share package is invalid.", nameof(package));
        }

        if (package.GameId != GameConfig.Id)
        {
            throw new InvalidOperationException($"Cannot import a {package.GameId} share package into {GameConfig.Id}.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var total = package.ChartIds.Length + package.Mods.Length;
        IReadOnlyList<BulkItemResult> chartResults = [];
        IReadOnlyList<BulkItemResult> modResults = [];

        if (package.ChartIds is not [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);
            var chartIds = package.ChartIds
                .Select(static chartId => chartId.ToString(CultureInfo.InvariantCulture))
                .ToArray();
            chartResults = await ChartManageService
                .DownloadChartsAsync(chartIds, CreatePhaseProgress(progress, 0, total), cancellationToken).ConfigureAwait(false);
        }

        if (package.Mods is not [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ModManageService.InitializeModsAsync().ConfigureAwait(false);
            var requests = package.Mods
                .Select(static mod => new ModInstallRequest(mod.Name, mod.IsDisabled))
                .ToArray();
            modResults = await ModManageService
                .InstallModsAsync(requests, CreatePhaseProgress(progress, package.ChartIds.Length, total), cancellationToken).ConfigureAwait(false);
        }

        return new GameShareImportResult(chartResults, modResults);
    }

    #region Injections

    public required IChartManageService ChartManageService { get; init; }
    public required IModManageService ModManageService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}
