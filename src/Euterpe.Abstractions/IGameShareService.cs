namespace Euterpe.Abstractions;

public interface IGameShareService
{
    string CreateChartShareLink(IReadOnlyCollection<int> chartIds);
    GameSharePackage? TryParseShareLink(string text);

    Task<IReadOnlyList<BulkItemResult>> ImportAsync(
        GameSharePackage package,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
