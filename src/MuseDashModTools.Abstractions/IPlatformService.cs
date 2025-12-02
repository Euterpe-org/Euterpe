namespace MuseDashModTools.Abstractions;

public interface IPlatformService
{
    /// <summary>
    ///     Get OS string for download link
    /// </summary>
    string OsString { get; }

    /// <summary>
    ///     Get Updater file name
    /// </summary>
    string UpdaterFileName { get; }

    /// <summary>
    ///     Get steam folder path
    /// </summary>
    /// <param name="steamFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder);

    /// <summary>
    ///     Get steam executable path
    /// </summary>
    /// <returns></returns>
    Task<string?> GetSteamExecPathAsync();

    /// <summary>
    ///     Get game folder path
    /// </summary>
    /// <param name="gameFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder);

    bool CheckIsValidSteamFolder(string folderPath);
    bool CheckIsValidSteamExecPath(string filePath);
    bool CheckIsValidGameFolder(string folderPath);

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