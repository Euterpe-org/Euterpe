namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    /// <summary>
    ///     Check Mod Tools Updates
    /// </summary>
    /// <param name="isUserClick"></param>
    /// <returns></returns>
    Task CheckUpdates(bool isUserClick = false);

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
    /// <returns>Is success</returns>
    Task<bool> DownloadMelonLoader(IProgress<double> downloadProgress);

    /// <summary>
    ///     Get MelonLoader File Size
    /// </summary>
    /// <returns>File size</returns>
    Task<long?> GetMelonLoaderFileSize();

    /// <summary>
    ///     Get Mod list
    /// </summary>
    /// <returns>Mod list</returns>
    Task<List<Mod>?> GetModListAsync();
}