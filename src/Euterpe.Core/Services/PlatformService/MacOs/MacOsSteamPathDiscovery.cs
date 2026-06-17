namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsSteamPathDiscovery : ISteamPathDiscovery
{
    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder) => throw new NotSupportedException();
    public Task<string?> GetSteamExecPathAsync() => throw new NotSupportedException();
    public bool CheckIsValidSteamFolder(string folderPath) => throw new NotSupportedException();
    public bool CheckIsValidSteamExecPath(string filePath) => throw new NotSupportedException();
}
