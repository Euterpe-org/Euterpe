namespace Euterpe.Abstractions;

public interface IDotNetSdkInstaller
{
    /// <summary>
    ///     Check the .NET SDK is installed.
    /// </summary>
    Task<bool> CheckInstalledAsync();

    /// <summary>
    ///     Install the .NET SDK.
    /// </summary>
    Task<bool> InstallAsync();
}