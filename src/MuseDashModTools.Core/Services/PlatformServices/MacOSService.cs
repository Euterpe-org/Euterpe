namespace MuseDashModTools.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsService : IPlatformService
{
    private static readonly string[] MacOsPaths = new[]
        {
            "Library/Application Support/Steam"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();

    public string OsString => "MacOS";
    public string UpdaterFileName => "Updater";
    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder) => throw new NotSupportedException();
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder) => throw new NotSupportedException();
    public bool CheckIsValidSteamFolder(string folderPath) => throw new NotSupportedException();
    public bool CheckIsValidGameFolder(string folderPath) => throw new NotSupportedException();
    public Task<bool> LaunchGameWithArgAsync(string gameId, string launchArgument) => throw new NotSupportedException();
    public Task<bool> LaunchGameWithArgsAsync(string gameId, IEnumerable<string> launchArguments) => throw new NotSupportedException();
    public Task<bool> InstallDotNetRuntimeAsync() => throw new NotSupportedException();
    public Task<bool> InstallDotNetSdkAsync() => throw new NotSupportedException();
    public void RevealFile(string filePath) => throw new NotSupportedException();
    public bool SetPathEnvironmentVariable() => throw new NotSupportedException();
    public Task OpenFolderAsync(string folderPath) => throw new NotSupportedException();
    public Task OpenFileAsync(string filePath) => throw new NotSupportedException();
    public Task OpenUriAsync(string uri) => throw new NotSupportedException();
}