namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGameModTemplateInstaller : IGameModTemplateInstaller
{
    public Task<bool> CheckInstalledAsync() => throw new NotSupportedException();
    public Task InstallAsync() => throw new NotSupportedException();
    public Task UninstallAsync() => throw new NotSupportedException();
}
