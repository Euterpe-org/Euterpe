namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsPlatformInfo : IPlatformInfo
{
    public string OsString => "osx";
}
