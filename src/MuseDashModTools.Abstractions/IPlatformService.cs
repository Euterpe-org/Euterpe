using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MuseDashModTools.Abstractions;

public interface IPlatformService
{
    public TopLevel TopLevel { get; init; }

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
    ///     Reveal file with path
    /// </summary>
    /// <param name="path"></param>
    void RevealFile(string path);

    /// <summary>
    ///     Set MD_DIRECTORY environment variable
    /// </summary>
    /// <returns></returns>
    bool SetPathEnvironmentVariable();

    /// <summary>
    ///     Open Folder
    /// </summary>
    /// <param name="folderPath"></param>
    void OpenFolder(string folderPath) => TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath));

    /// <summary>
    ///     Open File
    /// </summary>
    /// <param name="filePath"></param>
    void OpenFile(string filePath) => TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath));
}