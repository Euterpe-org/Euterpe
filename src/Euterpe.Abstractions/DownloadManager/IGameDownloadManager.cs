using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Abstractions;

public interface IGameDownloadManager
{
    Task DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task<string> DownloadChartAsync(string cid, CancellationToken cancellationToken = default);
    Task<string> UpdateChartAsync(string cid, IReadOnlyCollection<string> changedFiles, CancellationToken cancellationToken = default);
    Task<CheckChartUpdatesResponse> CheckChartUpdatesAsync(CheckChartUpdatesRequest request, CancellationToken cancellationToken = default);
    Task DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default);
    Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default);
    Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default);
}
