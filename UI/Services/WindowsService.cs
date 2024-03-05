using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Win32;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public sealed class WindowsService : IPlatformService
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private static readonly ImmutableList<string> WindowsPaths = new List<string>
        {
            @"Program Files\Steam\steamapps\common\Muse Dash",
            @"Program Files (x86)\Steam\steamapps\common\Muse Dash",
            @"Program Files\SteamLibrary\steamapps\common\Muse Dash",
            @"Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            @"Steam\steamapps\common\Muse Dash",
            @"SteamLibrary\steamapps\common\Muse Dash"
        }
        .SelectMany(path => DriveInfo.GetDrives().Select(drive => Path.Combine(drive.Name, path))).ToImmutableList();

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    public string OsString => "Windows";

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool GetGamePath(out string? folderPath)
    {
        folderPath = WindowsPaths.Find(Directory.Exists);

        if (folderPath is null)
        {
            if (!GetPathFromRegistry(out folderPath))
            {
                return false;
            }

            Logger.Information("Auto detected game path from Registry {Path}", folderPath);
            return true;
        }

        Logger.Information("Auto detected game path on Windows {Path}", folderPath);
        return true;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater.exe");

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public void OpenLogFolder(string logPath) => Process.Start("explorer.exe", "/select, " + logPath);

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool SetPathEnvironmentVariable()
    {
        try
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MD_NET6_DIRECTORY")))
            {
                Environment.SetEnvironmentVariable("MD_NET6_DIRECTORY", null);
            }

            if (Environment.GetEnvironmentVariable("MD_DIRECTORY") == Settings.MuseDashFolder)
            {
                return true;
            }

            Environment.SetEnvironmentVariable("MD_DIRECTORY", Settings.MuseDashFolder);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to set MD_DIRECTORY environment variable");
            return false;
        }
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public async ValueTask<bool> VerifyGameVersionAsync()
    {
        var version = FileVersionInfo.GetVersionInfo(Settings.MuseDashExePath).FileVersion;
        if (version is "2019.4.32.16288752")
        {
            Logger.Information("Correct game version {Version}", version);
            return true;
        }

        Logger.Error("Incorrect game version {Version}, showing error message box...", version);
        await MessageBoxService.ErrorMessageBox(MsgBox_Content_IncorrectVersion);
        return false;
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
}