namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsSystemAssociationSetup : ISystemAssociationSetup
{
    public Task RegisterAsync(string processPath) => throw new NotSupportedException();
}
