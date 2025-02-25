using System.Collections.Frozen;
using Avalonia.Platform.Storage;
using Microsoft.Win32;

namespace MuseDashModTools.Core;

internal sealed class WindowsService : IPlatformService
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private static readonly FrozenSet<string> WindowsPaths = new[]
        {
            @"Program Files\Steam\steamapps\common\Muse Dash",
            @"Program Files (x86)\Steam\steamapps\common\Muse Dash",
            @"Program Files\SteamLibrary\steamapps\common\Muse Dash",
            @"Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            @"Steam\steamapps\common\Muse Dash",
            @"SteamLibrary\steamapps\common\Muse Dash"
        }
        .SelectMany(path => DriveInfo.GetDrives().Select(drive => Path.Combine(drive.Name, path))).ToFrozenSet();

    public string OsString => "Windows";

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath)
    {
        folderPath = WindowsPaths.FirstOrDefault(Directory.Exists);

        if (folderPath is null)
        {
            if (!GetPathFromRegistry(out folderPath))
            {
                Logger.ZLogWarning($"Failed to auto detect game path on Windows");
                return false;
            }

            Logger.ZLogInformation($"Auto detect steam install on common path failed.\r\nDetected game path from Registry: {folderPath}");
            return true;
        }

        Logger.ZLogInformation($"Auto detected game path on Windows: {folderPath}");
        return true;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater.exe");

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public void RevealFile(string filePath)
    {
        Process.Start("explorer.exe", "/select, " + filePath);
        Logger.ZLogInformation($"Reveal file: {filePath}");
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool SetPathEnvironmentVariable()
    {
        try
        {
            if (Environment.GetEnvironmentVariable("MD_DIRECTORY") == Config.MuseDashFolder)
            {
                return true;
            }

            Environment.SetEnvironmentVariable("MD_DIRECTORY", Config.MuseDashFolder, EnvironmentVariableTarget.User);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to set MD_DIRECTORY environment variable");
            return false;
        }
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

    /// <summary>
    ///     Get game folder path from Registry
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns>Is success</returns>
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private static bool GetPathFromRegistry(out string folderPath)
    {
        folderPath = string.Empty;
        if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) is not string steamPath)
        {
            return false;
        }

        folderPath = Path.Combine(steamPath, "steamapps", "common", "Muse Dash");
        return Directory.Exists(folderPath);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required TopLevelProxy TopLevel { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsService> Logger { get; init; }

    #endregion Injections
}