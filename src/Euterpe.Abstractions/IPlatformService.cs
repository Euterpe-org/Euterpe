using System.Runtime.InteropServices;
using Euterpe.Contracts.Account;

namespace Euterpe.Abstractions;

public interface IPlatformService : IPlatformPathDiscovery, IPlatformDevEnvironment, IPlatformLauncher, IPlatformSecureStorage
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

    /// <summary>
    ///     Get MuseDash UID
    /// </summary>
    /// <returns></returns>
    Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync();
}