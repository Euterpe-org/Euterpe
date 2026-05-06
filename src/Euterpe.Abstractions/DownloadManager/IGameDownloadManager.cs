using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Abstractions;

public interface IGameDownloadManager
{
    Task DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default);
    Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default);
    Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default);
}