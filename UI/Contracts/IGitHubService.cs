namespace MuseDashModToolsUI.Contracts;

public interface IGitHubService
{
    /// <summary>
    ///     Check Mod Tools Updates
    /// </summary>
    /// <param name="isUserClick"></param>
    /// <returns></returns>
    Task CheckUpdatesAsync(bool isUserClick = false);

    /// <summary>
    ///     Download Chart
    /// </summary>
    /// <param name="id"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    Task DownloadChartAsync(int id, string path);

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
    Task<bool> DownloadMelonLoaderAsync(IProgress<double> downloadProgress);

    /// <summary>
    ///     Get Chart list
    /// </summary>
    /// <returns>Chart list</returns>
    Task<List<Chart>?> GetChartListAsync();

    /// <summary>
    ///     Get MelonLoader File Size
    /// </summary>
    /// <returns>File size</returns>
    Task<long?> GetMelonLoaderFileSizeAsync();

    /// <summary>
    ///     Get Mod list
    /// </summary>
    /// <returns>Mod list</returns>
    Task<List<Mod>?> GetModListAsync();
}