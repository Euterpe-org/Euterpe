using System.Collections.Frozen;
using Avalonia.Platform.Storage;
using Microsoft.Win32;

namespace MuseDashModTools.Core;

internal sealed class WindowsService : IPlatformService
{
    private const string DotnetRuntimeUrl = "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.exe";
    private const string DotnetSdkUrl = "https://aka.ms/dotnet/8.0/dotnet-sdk-win-x64.exe";

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
        .SelectMany(path => Environment.GetLogicalDrives().Select(drive => Path.Combine(drive, path))).ToFrozenSet();

    public string OsString => "Windows";

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool GetGamePath([NotNullWhen(true)] out string? folderPath)
    {
        folderPath = WindowsPaths.FirstOrDefault(Directory.Exists);

        if (folderPath is null)
        {
            Logger.ZLogWarning($"Auto detect steam install on common path failed.");
            if (!GetPathFromRegistry(out folderPath))
            {
                Logger.ZLogWarning($"Failed to auto detect game path on Windows");
                return false;
            }

            Logger.ZLogInformation($"Detected game path from Registry: {folderPath}");
            return true;
        }

        Logger.ZLogInformation($"Auto detected game path on Windows: {folderPath}");
        return true;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public string GetUpdaterFilePath(string folderPath) => Path.Combine(folderPath, "Updater.exe");

    public async Task<bool> InstallDotNetRuntimeAsync()
    {
        try
        {
            var tempFilePath = Path.GetTempFileName();
            await DownloadManager.DownloadFileAsync(DotnetRuntimeUrl, tempFilePath).ConfigureAwait(false);
            Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install .NET Runtime");
            return false;
        }
    }

    public async Task<bool> InstallDotNetSdkAsync()
    {
        try
        {
            var tempFilePath = Path.GetTempFileName();
            await DownloadManager.DownloadFileAsync(DotnetSdkUrl, tempFilePath).ConfigureAwait(false);
            Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install .NET SDK");
            return false;
        }
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("explorer.exe", $"/select, {filePath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );
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
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsService> Logger { get; init; }

    #endregion Injections
}