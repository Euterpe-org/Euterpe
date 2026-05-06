namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsDotNetSdkInstaller : IDotNetSdkInstaller
{
    public Task<bool> CheckInstalledAsync() => throw new NotSupportedException();
    public Task InstallAsync() => throw new NotSupportedException();
}