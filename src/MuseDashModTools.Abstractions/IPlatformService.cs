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

    #region Mod Develop

    /// <summary>
    ///     Install dotnet runtime
    /// </summary>
    /// <returns></returns>
    Task<bool> InstallDotNetRuntimeAsync();

    /// <summary>
    ///     Install dotnet sdk
    /// </summary>
    /// <returns></returns>
    Task<bool> InstallDotNetSdkAsync();

    /// <summary>
    ///     Install Mod Template
    /// </summary>
    /// <returns></returns>
    Task InstallModTemplateAsync() =>
        Cli.Wrap("dotnet")
            .WithArguments(["new", "install", "MuseDash.Mod.Template"])
            .ExecuteAsync();

    /// <summary>
    ///     Uninstall Mod Template
    /// </summary>
    /// <returns></returns>
    Task UninstallModTemplateAsync() =>
        Cli.Wrap("dotnet")
            .WithArguments(["new", "uninstall", "MuseDash.Mod.Template"])
            .ExecuteAsync();

    /// <summary>
    ///     Set MD_DIRECTORY environment variable
    /// </summary>
    /// <returns></returns>
    bool SetPathEnvironmentVariable();

    #endregion Mod Develop

    #region File Operations

    /// <summary>
    ///     Reveal file with path
    /// </summary>
    /// <param name="filePath"></param>
    void RevealFile(string filePath);

    /// <summary>
    ///     Open Folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    Task OpenFolderAsync(string folderPath);

    /// <summary>
    ///     Open File
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task OpenFileAsync(string filePath);

    /// <summary>
    ///     Open Uri
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task OpenUriAsync(string uri);

    #endregion File Operations
}