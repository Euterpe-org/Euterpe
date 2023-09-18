using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    /// <summary>
    ///     Check Mod Tools Updates
    /// </summary>
    /// <param name="userClick"></param>
    /// <returns></returns>
    Task CheckUpdates(bool userClick = false);

    /// <summary>
    ///     Download Mod
    /// </summary>
    /// <param name="link">Download link</param>
    /// <param name="path">File path</param>
    /// <returns></returns>
    Task DownloadModAsync(string link, string path);

    /// <summary>
    ///     Download MelonLoader
    /// </summary>
    /// <param name="downloadProgress"></param>
    /// <returns>Success</returns>
    Task<bool> DownloadMelonLoader(IProgress<double> downloadProgress);

    /// <summary>
    ///     Get MelonLoader File Size
    /// </summary>
    /// <returns>Success</returns>
    Task<long?> GetMelonLoaderFileSize();

    /// <summary>
    ///     Get Mod list
    /// </summary>
    /// <returns>Mod list</returns>
    Task<List<Mod>?> GetModListAsync();
}