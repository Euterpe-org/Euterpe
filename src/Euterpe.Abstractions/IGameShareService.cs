namespace Euterpe.Abstractions;

public interface IGameShareService
{
    string CreateChartShareLink(IReadOnlyCollection<int> chartIds);
    Task<string?> CreateInstalledModsShareLinkAsync();
    GameSharePackage? TryParseShareLink(string text);
    Task<GameShareImportResult> ImportAsync(GameSharePackage package, IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
