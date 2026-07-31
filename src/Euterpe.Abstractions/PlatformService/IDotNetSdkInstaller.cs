namespace Euterpe.Abstractions;

[PlatformService(ServiceRegistrationLifetime.AppSingleton)]
public interface IDotNetSdkInstaller
{
    /// <summary>
    ///     Check the .NET SDK is installed.
    /// </summary>
    Task<bool> CheckInstalledAsync();

    /// <summary>
    ///     Install the .NET SDK. Throws on failure.
    /// </summary>
    Task InstallAsync();
}
