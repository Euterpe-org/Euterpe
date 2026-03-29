using Euterpe.Contracts.Account;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsService : IPlatformService
{
    private static readonly string[] MacOsPaths = new[]
        {
            "Library/Application Support/Steam"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();

    public string OsString => "osx";
    public string UpdaterFileName => "Updater";
    public Task SetupDeepLinkAsync(string processPath) => throw new NotSupportedException();
    public Task<MuseDashUidRequest?> GetMuseDashUserIdAsync() => throw new NotSupportedException();
    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder) => throw new NotSupportedException();
    public Task<string?> GetSteamExecPathAsync() => throw new NotSupportedException();
    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder) => throw new NotSupportedException();
    public bool CheckIsValidSteamFolder(string folderPath) => throw new NotSupportedException();
    public bool CheckIsValidSteamExecPath(string filePath) => throw new NotSupportedException();
    public Task<bool> CheckDotNetSdkInstalledAsync() => throw new NotSupportedException();
    public Task<bool> CheckModTemplateInstalledAsync() => throw new NotSupportedException();
    public Task<bool> CheckDotNetRuntimeInstalledAsync() => throw new NotSupportedException();
    public bool CheckIsValidGameFolder(string folderPath) => throw new NotSupportedException();
    public Task<bool> InstallDotNetRuntimeAsync() => throw new NotSupportedException();
    public Task<bool> InstallDotNetSdkAsync() => throw new NotSupportedException();
    public Task InstallModTemplateAsync() => throw new NotSupportedException();
    public Task UninstallModTemplateAsync() => throw new NotSupportedException();
    public void RevealFile(string filePath) => throw new NotSupportedException();
    public bool CheckPathEnvironmentVariableSet() => throw new NotSupportedException();
    public bool SetPathEnvironmentVariable() => throw new NotSupportedException();
    public Task OpenFolderAsync(string folderPath) => throw new NotSupportedException();
    public Task OpenFileAsync(string filePath) => throw new NotSupportedException();
    public Task OpenUriAsync(string uri) => throw new NotSupportedException();
    public Task SaveTokensAsync(string accessToken, string refreshToken) => throw new NotSupportedException();
    public Task<(string AccessToken, string RefreshToken)?> LoadTokensAsync() => throw new NotSupportedException();
    public Task ClearTokensAsync() => throw new NotSupportedException();
}