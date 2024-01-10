namespace MuseDashModToolsUI.Services;

public sealed class MacOsService : IPlatformService
{
    public string OsString => "MacOS";
    public bool GetGamePath(out string? folderPath) => throw new NotImplementedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotImplementedException();
    public void OpenLogFolder(string logPath) => throw new NotImplementedException();
    public Task<bool> VerifyGameVersionAsync() => throw new NotImplementedException();
}