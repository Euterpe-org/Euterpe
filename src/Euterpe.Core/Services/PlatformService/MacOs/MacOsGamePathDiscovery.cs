namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGamePathDiscovery : IGamePathDiscovery
{
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder) => throw new NotSupportedException();
    public bool CheckIsValidGameFolder([NotNullWhen(true)] string? folderPath) => throw new NotSupportedException();
}