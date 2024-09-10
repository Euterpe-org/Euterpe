namespace MuseDashModToolsUI.Contracts.Downloads;

public interface IDownloadProvider
{
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);
}