namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsPlatformInfo : IPlatformInfo
{
    public string OsString => "win";
    public string UpdaterFileName => "Updater.exe";
}