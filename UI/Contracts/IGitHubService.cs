using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    Task CheckUpdates(bool userClick = false);
    Task DownloadModAsync(string link, string path);
    Task<bool> DownloadMelonLoader(IProgress<double> downloadProgress);
    Task<long?> GetMelonLoaderFileSize();
    Task<List<Mod>?> GetModListAsync();
}