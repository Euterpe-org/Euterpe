using System.Runtime.InteropServices;

namespace Euterpe.Abstractions;

public interface IPlatformService
{
    /// <summary>
    ///     Fixed deep link scheme shared by all platforms.
    /// </summary>
    const string DeepLinkScheme = "euterpe";

    /// <summary>
    ///     Get OS string
    /// </summary>
    string OsString { get; }

    /// <summary>
    ///     Get Updater file name
    /// </summary>
    string UpdaterFileName { get; }

    /// <summary>
    ///     Get architecture string
    /// </summary>
    string ArchitectureString =>
        RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => "unknown"
        };

    /// <summary>
    ///     Get runtime identifier
    /// </summary>
    string RuntimeIdentifier => $"{OsString}-{ArchitectureString}";

    /// <summary>
    ///     Setup deep link handler for the current platform.
    /// </summary>
    /// <param name="processPath"></param>
    Task SetupDeepLinkAsync(string processPath);

    #region Path Discovery

    /// <summary>
    ///     Get steam folder path
    /// </summary>
    /// <param name="steamFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder);

    /// <summary>
    ///     Check is valid Steam folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    bool CheckIsValidSteamFolder(string folderPath);

    /// <summary>
    ///     Get game folder path
    /// </summary>
    /// <param name="gameFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder);

    /// <summary>
    ///     Check is valid game folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    bool CheckIsValidGameFolder(string folderPath);

    /// <summary>
    ///     Get steam executable path
    /// </summary>
    /// <returns></returns>
    Task<string?> GetSteamExecPathAsync();

    /// <summary>
    ///     Check is valid Steam executable path
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool CheckIsValidSteamExecPath(string filePath);

    #endregion Path Discovery

    #region Dev Environment

    /// <summary>
    ///     Check dotnet runtime installed
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckDotNetRuntimeInstalledAsync();

    /// <summary>
    ///     Check dotnet SDK installed
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckDotNetSdkInstalledAsync();

    /// <summary>
    ///     Check mod template installed
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckModTemplateInstalledAsync();

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
    Task InstallModTemplateAsync();

    /// <summary>
    ///     Uninstall Mod Template
    /// </summary>
    /// <returns></returns>
    Task UninstallModTemplateAsync();

    /// <summary>
    ///     Check if MD_DIRECTORY environment variable exists
    /// </summary>
    /// <returns></returns>
    bool CheckPathEnvironmentVariableSet();

    /// <summary>
    ///     Set MD_DIRECTORY environment variable
    /// </summary>
    /// <returns></returns>
    bool SetPathEnvironmentVariable();

    #endregion Dev Environment

    #region Launcher

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

    #endregion Launcher
}