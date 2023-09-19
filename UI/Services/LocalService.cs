using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AssetsTools.NET.Extra;
using DialogHostAvalonia;
using MelonLoader;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

#pragma warning disable CS8618

public partial class LocalService : ILocalService
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private static readonly ImmutableList<string> WindowsPaths = new List<string>
        {
            @"Program Files\Steam\steamapps\common\Muse Dash",
            @"Program Files (x86)\Steam\steamapps\common\Muse Dash",
            @"Program Files\SteamLibrary\steamapps\common\Muse Dash",
            @"Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            @"Steam/steamapps\common\Muse Dash",
            @"SteamLibrary\steamapps\common\Muse Dash"
        }
        .SelectMany(path => DriveInfo.GetDrives().Select(drive => Path.Combine(drive.Name, path))).ToImmutableList();

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static readonly ImmutableList<string> LinuxPaths = new List<string>
        {
            ".local/share/Steam/steamapps/common/Muse Dash",
            ".steam/steam/steamapps/common/Muse Dash"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToImmutableList();

    public IDownloadWindowViewModel DownloadWindowViewModel { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISavingService SavingService { get; init; }
    private bool IsValidPath { get; set; }

    public async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(SavingService.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(SavingService.Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_InstallMelonLoader);
        if (install)
            await OnInstallMelonLoader();
    }

    public async Task CheckValidPath()
    {
        Logger.Information("Checking valid path...");
        await CheckGameFileExist();

        try
        {
            if (OperatingSystem.IsWindows() && !await VerifyGameVersion()) return;
            await CreateFiles();
            IsValidPath = true;
            Logger.Information("Path verified {Path}", SavingService.Settings.MuseDashFolder);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exe verify failed, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ExeVerifyFailed);
        }
    }

    public IEnumerable<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll")
        .ToList();

    public async Task<bool> LaunchUpdater(string link)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterFilePath = GetUpdaterFilePath(currentDirectory);
        var updaterTargetPath = GetUpdaterFilePath(updaterTargetFolder);

        if (!await CheckUpdaterFilesExist(updaterFilePath, updaterTargetFolder)) return false;

        try
        {
            File.Copy(updaterFilePath, updaterTargetPath, true);
            Logger.Information("Copy Updater to Update folder success");
        }
        catch (Exception ex)
        {
            Logger.Information(ex, "Copy Updater to Update folder failed");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_CopyUpdaterFailed, ex);
        }

        Process.Start(updaterTargetPath, new[] { link, currentDirectory });
        return true;
    }

    public Mod? LoadMod(string filePath)
    {
        var mod = new Mod
        {
            IsDisabled = filePath.EndsWith(".disabled")
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(filePath)[..^9] : Path.GetFileName(filePath);
        var assembly = Assembly.Load(File.ReadAllBytes(filePath));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);
        if (attribute is null) return null;

        mod.Name = attribute.Name;
        mod.LocalVersion = attribute.Version;

        if (mod.Name == null || mod.LocalVersion == null) return null;

        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(filePath);
        Logger.Information("Local mod {Name} loaded. File name {FileName}", mod.Name, mod.FileName);
        return mod;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public bool GetPathOnLinux(out string? folderPath)
    {
        folderPath = LinuxPaths.FirstOrDefault(Directory.Exists);
        return folderPath is not null;
    }

    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    public bool GetPathOnWindows(out string? folderPath)
    {
        folderPath = WindowsPaths.FirstOrDefault(Directory.Exists);
        return folderPath is not null || GetPathFromRegistry(out folderPath);
    }

    public async Task OnInstallMelonLoader()
    {
        if (!IsValidPath) return;
        Logger.Information("Showing MelonLoader download window...");
        await DialogHost.Show(DownloadWindowViewModel, "DownloadWindowDialog",
            (object _, DialogOpenedEventArgs _) => DownloadWindowViewModel.InstallMelonLoader());
    }

    public async Task OnUninstallMelonLoader()
    {
        if (!IsValidPath) return;
        if (!await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_UninstallMelonLoader)) return;
        var versionFile = Path.Join(SavingService.Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(SavingService.Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(SavingService.Settings.MelonLoaderFolder))
        {
            try
            {
                Directory.Delete(SavingService.Settings.MelonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                Logger.Information("MelonLoader uninstalled successfully");
                await MessageBoxService.SuccessMessageBox(MsgBox_Content_UninstallMelonLoaderSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "MelonLoader uninstall failed, showing error message box...");
                await MessageBoxService.ErrorMessageBox(MsgBox_Content_UninstallMelonLoaderFailed);
            }
        }
        else
        {
            Logger.Error("MelonLoader folder not found, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoMelonLoaderFolder);
        }
    }

    public async Task OpenModsFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SavingService.OnChoosePath();
            return;
        }

        Logger.Information("Opening mods folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SavingService.Settings.ModsFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenUserDataFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SavingService.OnChoosePath();
            return;
        }

        Logger.Information("Opening UserData folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SavingService.Settings.UserDataFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenLogFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SavingService.OnChoosePath();
            return;
        }

        var logPath = Path.Combine(SavingService.Settings.MelonLoaderFolder, "Latest.log");
        Logger.Information("Opening Log folder...");
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", "/select, " + logPath);
        if (OperatingSystem.IsLinux())
            Process.Start("xdg-open", logPath);
    }

    public async Task<string> ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(SavingService.Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion");
            Logger.Information("Game version read successfully: {BundleVersion}", bundleVersion!.AsString);
            return bundleVersion.AsString;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Read game version failed, showing error message box...");
            await MessageBoxService.ErrorMessageBox(string.Format(MsgBox_Content_ReadGameVersionFailed, bundlePath));
            Environment.Exit(0);
        }

        return string.Empty;
    }
}