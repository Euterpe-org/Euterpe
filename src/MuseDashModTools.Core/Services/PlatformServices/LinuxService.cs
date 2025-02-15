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

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater");

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public void RevealFile(string filePath)
    {
        Process.Start("xdg-open", filePath);
        Logger.ZLogInformation($"Reveal file: {filePath}");
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool SetPathEnvironmentVariable() => false;

    public void OpenFolder(string folderPath)
    {
        TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath));
        Logger.ZLogInformation($"Open folder: {folderPath}");
    }

    public void OpenFile(string filePath)
    {
        TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath));
        Logger.ZLogInformation($"Open file: {filePath}");
    }

    #region Injections

    [UsedImplicitly]
    public TopLevelProxy TopLevel { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<LinuxService> Logger { get; init; } = null!;

    #endregion Injections
}