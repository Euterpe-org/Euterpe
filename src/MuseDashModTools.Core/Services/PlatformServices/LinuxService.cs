using System.Collections.Frozen;
using Avalonia.Platform.Storage;

namespace MuseDashModTools.Core;

internal sealed class LinuxService : IPlatformService
{
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static readonly FrozenSet<string> LinuxPaths = new[]
        {
            ".local/share/Steam/steamapps/common/Muse Dash",
            ".steam/steam/steamapps/common/Muse Dash"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToFrozenSet();

    public string OsString => "Linux";
    public string UpdaterFileName => "Updater";

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath)
    {
        folderPath = LinuxPaths.FirstOrDefault(Directory.Exists);
        if (folderPath is null)
        {
            Logger.ZLogWarning($"Failed to auto detect game path on Linux");
            return false;
        }

        Logger.ZLogInformation($"Auto detected game path on Linux: {folderPath}");
        return true;
    }

    public Task<bool> InstallDotNetRuntimeAsync() => throw new NotSupportedException();

    public Task<bool> InstallDotNetSdkAsync() => throw new NotSupportedException();

    public bool SetPathEnvironmentVariable() => throw new NotSupportedException();

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("xdg-open", filePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );

        Logger.ZLogInformation($"Reveal file: {filePath}");
    }

    public async Task OpenFolderAsync(string folderPath)
    {
        await TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open folder: {folderPath}");
    }

    public async Task OpenFileAsync(string filePath)
    {
        await TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open file: {filePath}");
    }

    public async Task OpenUriAsync(string uri)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(uri)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open uri: {uri}");
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater");

    #region Injections

    [UsedImplicitly]
    public required TopLevelProxy TopLevel { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxService> Logger { get; init; }

    #endregion Injections
}