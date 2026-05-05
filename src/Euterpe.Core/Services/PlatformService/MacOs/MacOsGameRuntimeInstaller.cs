namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGameRuntimeInstaller : IGameRuntimeInstaller
{
    public Task<bool> CheckInstalledAsync() => throw new NotSupportedException();
    public Task<bool> InstallAsync() => throw new NotSupportedException();
}