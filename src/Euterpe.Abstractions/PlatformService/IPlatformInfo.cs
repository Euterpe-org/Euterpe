using System.Runtime.InteropServices;

namespace Euterpe.Abstractions;

public interface IPlatformInfo
{
    /// <summary>
    ///     Get OS string
    /// </summary>
    string OsString { get; }

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
}
