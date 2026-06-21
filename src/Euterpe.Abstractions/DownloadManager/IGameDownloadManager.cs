using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Abstractions;

public interface IGameDownloadManager
{
    // Asset downloads
    Task DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default);

    // Chart operations
    Task<string> DownloadChartAsync(string cid, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default);
    Task<string> UpdateChartAsync(string cid, IReadOnlyCollection<string> changedFiles, IReadOnlyCollection<string> deletedFiles, CancellationToken cancellationToken = default);
    Task<CheckChartUpdatesResponse> CheckChartUpdatesAsync(CheckChartUpdatesRequest request, CancellationToken cancellationToken = default);

    // Catalog fetches
    Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default);
    Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default);
}
