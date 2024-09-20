using Avalonia.Platform.Storage;

namespace MuseDashModTools.Contracts;

public interface IPlatformService
{
    /// <summary>
    ///     Get OS string for download link
    /// </summary>
    string OsString { get; }

    /// <summary>
    ///     Get game folder path
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns>Is success</returns>
    bool GetGamePath([NotNullWhen(true)] out string? folderPath);

    /// <summary>
    ///     Get Updater file path
    /// </summary>
    /// <param name="folderPath">Created Updater folder path</param>
    /// <returns>Updater file path</returns>
    string GetUpdaterFilePath(string folderPath);

    /// <summary>
    ///     Open file with path
    /// </summary>
    /// <param name="path"></param>
    Task OpenFileAsync(string path) => GetLauncher().LaunchFileInfoAsync(new FileInfo(path));

    /// <summary>
    ///     Open folder with path
    /// </summary>
    /// <param name="path"></param>
    Task OpenFolderAsync(string path) => GetLauncher().LaunchDirectoryInfoAsync(new DirectoryInfo(path));

    /// <summary>
    ///     Open a URL in the default browser
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task OpenUrlAsync(string url) => GetLauncher().LaunchUriAsync(new Uri(url));

    /// <summary>
    ///     Reveal file with path
    /// </summary>
    /// <param name="path"></param>
    void RevealFile(string path);

    /// <summary>
    ///     Set MD_DIRECTORY environment variable
    /// </summary>
    /// <returns></returns>
    bool SetPathEnvironmentVariable();
}