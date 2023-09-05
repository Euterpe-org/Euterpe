using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    Task<long?> GetMelonLoaderFileSize();
    Task<List<Mod>?> GetModListAsync();
    Task DownloadModAsync(string link, string path);
    Task DownloadMelonLoader(string path, IProgress<double> downloadProgress);
    Task CheckUpdates(bool userClick = false);
}