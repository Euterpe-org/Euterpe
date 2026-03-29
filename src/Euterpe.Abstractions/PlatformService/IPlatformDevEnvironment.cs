namespace Euterpe.Abstractions;

public interface IPlatformDevEnvironment
{
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
}