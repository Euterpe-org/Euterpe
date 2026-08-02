namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGameRuntimeInstaller : IGameRuntimeInstaller
{
    public Task<bool> CheckInstalledAsync(string runtimeVersion) => throw new NotSupportedException();
    public Task InstallAsync(string runtimeVersion) => throw new NotSupportedException();
}
