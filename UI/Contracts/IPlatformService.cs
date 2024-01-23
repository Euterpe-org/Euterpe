namespace MuseDashModToolsUI.Contracts;

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
    bool GetGamePath(out string? folderPath);

    /// <summary>
    ///     Get Updater file path
    /// </summary>
    /// <param name="folderPath">Created Updater folder path</param>
    /// <returns>Updater file path</returns>
    string GetUpdaterFilePath(string folderPath);

    /// <summary>
    ///     Open folder of Latest.log
    /// </summary>
    /// <param name="logPath"></param>
    void OpenLogFolder(string logPath);

    /// <summary>
    ///     Verify game exe version
    /// </summary>
    /// <returns>Is correct version</returns>
    ValueTask<bool> VerifyGameVersionAsync();
}