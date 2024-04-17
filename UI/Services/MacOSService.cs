namespace MuseDashModToolsUI.Services;

public sealed class MacOsService : IPlatformService
{
    public string OsString => "MacOS";
    public bool GetGamePath(out string? folderPath) => throw new NotImplementedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotImplementedException();
    public void OpenFile(string path) => throw new NotImplementedException();
    public bool SetPathEnvironmentVariable() => throw new NotImplementedException();
    public ValueTask<bool> VerifyGameVersionAsync() => throw new NotImplementedException();
}