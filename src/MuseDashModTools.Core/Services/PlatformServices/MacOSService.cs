namespace MuseDashModTools.Core;

internal sealed class MacOsService : IPlatformService
{
    public string OsString => "MacOS";
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath) => throw new NotImplementedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotImplementedException();
    public void RevealFile(string filePath) => throw new NotImplementedException();
    public bool SetPathEnvironmentVariable() => throw new NotImplementedException();
    public Task OpenFolderAsync(string folderPath) => throw new NotImplementedException();
    public Task OpenFileAsync(string filePath) => throw new NotImplementedException();
    public Task OpenUriAsync(string uri) => throw new NotImplementedException();
}