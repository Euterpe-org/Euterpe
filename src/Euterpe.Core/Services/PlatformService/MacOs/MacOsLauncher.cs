namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsLauncher : IPlatformLauncher
{
    public Task OpenFileAsync(string filePath) => throw new NotSupportedException();
    public Task OpenFolderAsync(string folderPath) => throw new NotSupportedException();
    public Task OpenUriAsync(string uri) => throw new NotSupportedException();
    public Task RevealFileAsync(string filePath) => throw new NotSupportedException();
}
