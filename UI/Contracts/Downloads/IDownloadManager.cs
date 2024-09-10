namespace MuseDashModToolsUI.Contracts.Downloads;

public interface IDownloadManager
{
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);
}