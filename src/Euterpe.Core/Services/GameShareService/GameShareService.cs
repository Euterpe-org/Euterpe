namespace Euterpe.Core;

internal sealed class GameShareService : IGameShareService
{
    private const string ShareLinkPrefix = ISystemAssociationSetup.DeepLinkScheme + "://share/";

    public string CreateChartShareLink(IReadOnlyCollection<int> chartIds)
    {
        var package = new GameSharePackage
        {
            SchemaVersion = Manifest.CurrentSchema,
            GameId = GameConfig.Id,
            ChartIds = [.. chartIds]
        };

        return ShareLinkPrefix + MessagePackSerialization.SerializeGameSharePackage(package).ToBase64Url();
    }

    public GameSharePackage? TryParseShareLink(string text)
    {
        var code = text.Trim();
        if (code.StartsWith(ShareLinkPrefix, StringComparison.OrdinalIgnoreCase))
        {
            code = code[ShareLinkPrefix.Length..];
        }

        try
        {
            var package = MessagePackSerialization.DeserializeGameSharePackage(code.FromBase64Url());
            return package.SchemaVersion == Manifest.CurrentSchema ? package : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<BulkItemResult>> ImportAsync(
        GameSharePackage package,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (package.GameId != GameConfig.Id)
        {
            throw new InvalidOperationException($"Cannot import a {package.GameId} share package into {GameConfig.Id}.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);
        var chartIds = package.ChartIds
            .Select(static chartId => chartId.ToString(CultureInfo.InvariantCulture))
            .ToArray();

        return await ChartManageService.DownloadChartsAsync(chartIds, progress, cancellationToken).ConfigureAwait(false);
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }

    #endregion Injections
}
