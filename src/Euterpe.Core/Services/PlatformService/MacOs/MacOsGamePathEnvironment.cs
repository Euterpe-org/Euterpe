namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGamePathEnvironment : IGamePathEnvironment
{
    public bool IsSet() => throw new NotSupportedException();
    public bool Set() => throw new NotSupportedException();
}
