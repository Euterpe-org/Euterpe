namespace Euterpe.Models.Sharing;

public sealed record GameShareImportResult(
    IReadOnlyList<BulkItemResult> ChartResults,
    IReadOnlyList<BulkItemResult> ModResults);
