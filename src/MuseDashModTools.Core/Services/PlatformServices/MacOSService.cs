namespace MuseDashModTools.Core;

internal sealed class MacOsService : IPlatformService
{
    public string OsString => "MacOS";
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath) => throw new NotSupportedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotSupportedException();
    public Task<bool> InstallDotNetRuntimeAsync() => throw new NotSupportedException();
    public Task<bool> InstallDotNetSdkAsync() => throw new NotSupportedException();
    public void RevealFile(string filePath) => throw new NotSupportedException();
    public bool SetPathEnvironmentVariable() => throw new NotSupportedException();
    public Task OpenFolderAsync(string folderPath) => throw new NotSupportedException();
    public Task OpenFileAsync(string filePath) => throw new NotSupportedException();
    public Task OpenUriAsync(string uri) => throw new NotSupportedException();
}