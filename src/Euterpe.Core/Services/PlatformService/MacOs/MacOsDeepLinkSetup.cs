namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsDeepLinkSetup : IDeepLinkSetup
{
    public Task SetupDeepLinkAsync(string processPath) => throw new NotSupportedException();
}
