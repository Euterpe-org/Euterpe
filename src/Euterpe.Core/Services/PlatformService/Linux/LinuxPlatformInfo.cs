namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxPlatformInfo : IPlatformInfo
{
    public string OsString => "linux";
    public string UpdaterFileName => "Updater";
}
