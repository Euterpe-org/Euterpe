namespace MuseDashModToolsUI.Contracts.Downloads;

public interface IDownloadService
{
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadMelonLoaderAsync(CancellationToken cancellationToken = default);
}