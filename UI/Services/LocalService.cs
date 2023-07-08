using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AssetsTools.NET.Extra;
using DialogHostAvalonia;
using MelonLoader;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

#pragma warning disable CS8618

public class LocalService : ILocalService
{
    public IMessageBoxService MessageBoxService { get; init; }
    public IDownloadWindowViewModel DownloadWindowViewModel { get; init; }
    public ILogger Logger { get; init; }
    public ISettingService SettingService { get; init; }
    private bool IsValidPath { get; set; }

    public IEnumerable<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll")
        .ToList();

    public Mod? LoadMod(string filePath)
    {
        var mod = new Mod
        {
            IsDisabled = filePath.EndsWith(".disabled")
        };

        mod.FileName = mod.IsDisabled ? Path.GetFileName(filePath)[..^9] : Path.GetFileName(filePath);
        var assembly = Assembly.Load(File.ReadAllBytes(filePath));
        var attribute = MelonUtils.PullAttributeFromAssembly<MelonInfoAttribute>(assembly);

        mod.Name = attribute.Name;
        mod.LocalVersion = attribute.Version;

        if (mod.Name == null || mod.LocalVersion == null) return null;

        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(filePath);
        Logger.Information("Local mod {Name} loaded. File name {FileName}", mod.Name, mod.FileName);
        return mod;
    }

    public async Task<bool> CheckValidPath()
    {
        Logger.Information("Checking valid path...");
        var exePath = Path.Join(SettingService.Settings.MuseDashFolder, "MuseDash.exe");
        var gameAssemblyPath = Path.Join(SettingService.Settings.MuseDashFolder, "GameAssembly.dll");

        if (!File.Exists(exePath) || !File.Exists(gameAssemblyPath))
        {
            Logger.Error("No game files found, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoExeFound.Localize());
            await SettingService.OnChoosePath();
            return false;
        }

        try
        {
            var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
            if (version is not "2019.4.32.16288752")
            {
                await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_IncorrectVersion.Localize());
                IsValidPath = false;
                return IsValidPath;
            }

            if (!Directory.Exists(SettingService.Settings.ModsFolder))
            {
                Directory.CreateDirectory(SettingService.Settings.ModsFolder);
                Logger.Information("Mods folder not found, created");
            }

            if (!Directory.Exists(SettingService.Settings.UserDataFolder))
            {
                Directory.CreateDirectory(SettingService.Settings.UserDataFolder);
                Logger.Information("UserData folder not found, created");
            }

            var cfgFilePath = Path.Join(SettingService.Settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
            await File.WriteAllTextAsync(cfgFilePath, Environment.ProcessPath);
            Logger.Information("Write path into cfg file successfully");

            IsValidPath = true;
            Logger.Information("Path verified");
            return IsValidPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exe verify failed, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_ExeVerifyFailed.Localize());
            await SettingService.OnChoosePath();
            IsValidPath = false;
            return IsValidPath;
        }
    }

    public async Task<string> ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(SettingService.Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
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
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_ReadGameVersionFailed.Localize(), bundlePath));
            Environment.Exit(0);
        }

        return string.Empty;
    }

    public async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(SettingService.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(SettingService.Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await MessageBoxService.CreateConfirmMessageBox(MsgBox_Title_Notice, MsgBox_Content_InstallMelonLoader.Localize());
        if (install)
            await OnInstallMelonLoader();
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
        if (!await MessageBoxService.CreateConfirmMessageBox(MsgBox_Content_UninstallMelonLoader.Localize())) return;
        var versionFile = Path.Join(SettingService.Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(SettingService.Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(SettingService.Settings.MelonLoaderFolder))
        {
            try
            {
                Directory.Delete(SettingService.Settings.MelonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                Logger.Information("MelonLoader uninstalled successfully");
                await MessageBoxService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_UninstallMelonLoaderSuccess.Localize());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "MelonLoader uninstall failed, showing error message box...");
                await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_UninstallMelonLoaderFailed.Localize());
            }
        }
        else
        {
            Logger.Error("MelonLoader folder not found, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoMelonLoaderFolder.Localize());
        }
    }

    public async Task OpenModsFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SettingService.OnChoosePath();
            return;
        }

        Logger.Information("Opening mods folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SettingService.Settings.ModsFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenUserDataFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SettingService.OnChoosePath();
            return;
        }

        Logger.Information("Opening UserData folder...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SettingService.Settings.UserDataFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenLogFolder()
    {
        if (!IsValidPath)
        {
            Logger.Error("Not valid path, showing error message box...");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await SettingService.OnChoosePath();
            return;
        }

        var logPath = Path.Combine(SettingService.Settings.MelonLoaderFolder, "Latest.log");
        Logger.Information("Opening Log folder...");
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", "/select, " + logPath);
        if (OperatingSystem.IsLinux())
            Process.Start("xdg-open", logPath);
    }
}