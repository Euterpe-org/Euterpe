namespace MuseDashModTools.Abstractions;

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