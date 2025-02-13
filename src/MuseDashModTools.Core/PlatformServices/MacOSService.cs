using Avalonia.Controls;

namespace MuseDashModTools.Core;

internal sealed class MacOsService : IPlatformService
{
    public TopLevel TopLevel { get; init; } = null!;
    public string OsString => "MacOS";
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath) => throw new NotImplementedException();
    public string GetUpdaterFilePath(string folderPath) => throw new NotImplementedException();
    public void RevealFile(string path) => throw new NotImplementedException();
    public bool SetPathEnvironmentVariable() => throw new NotImplementedException();
}