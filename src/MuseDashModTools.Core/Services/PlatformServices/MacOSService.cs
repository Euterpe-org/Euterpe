namespace MuseDashModTools.Core;

internal sealed class MacOsService : IPlatformService
{
    public string OsString => "MacOS";
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath) => throw new NotImplementedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotImplementedException();
    public void RevealFile(string path) => throw new NotImplementedException();
    public bool SetPathEnvironmentVariable() => throw new NotImplementedException();
    public void OpenFolder(string folderPath) => throw new NotImplementedException();
    public void OpenFile(string filePath) => throw new NotImplementedException();
}