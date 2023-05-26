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
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class LocalService : ILocalService
{
    private readonly IDialogueService _dialogueService;
    private readonly IDownloadWindowViewModel _downloadWindowViewModel;
    private readonly ISettingService _settings;

    private bool IsValidPath { get; set; }

    public LocalService(IDialogueService dialogueService, ISettingService settings, IDownloadWindowViewModel downloadWindowViewModel)
    {
        _dialogueService = dialogueService;
        _settings = settings;
        _downloadWindowViewModel = downloadWindowViewModel;
    }

    public List<string> GetModFiles(string path) => Directory.GetFiles(path)
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
        return mod;
    }

    public async Task<bool> CheckValidPath()
    {
        var exePath = Path.Join(_settings.Settings.MuseDashFolder, "MuseDash.exe");
        var gameAssemblyPath = Path.Join(_settings.Settings.MuseDashFolder, "GameAssembly.dll");
        var userDataPath = Path.Join(_settings.Settings.MuseDashFolder, "UserData");
        if (!File.Exists(exePath) || !File.Exists(gameAssemblyPath))
        {
            await _dialogueService.CreateErrorMessageBox("Couldn't find MuseDash.exe or GameAssembly.dll\nPlease choose the right folder");
            await _settings.OnChoosePath();
        }

        try
        {
            var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
            if (version is not "2019.4.32.16288752")
            {
                await _dialogueService.CreateErrorMessageBox(
                    "Muse Dash.exe is not correct version \nAre you using a pirated or modified version?");
                IsValidPath = false;
                return IsValidPath;
            }

            if (!Directory.Exists(_settings.Settings.ModsFolder))
                Directory.CreateDirectory(_settings.Settings.ModsFolder);

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            var cfgFilePath = Path.Join(_settings.Settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
            if (!File.Exists(cfgFilePath))
            {
                await File.WriteAllTextAsync(cfgFilePath, Environment.ProcessPath);
            }
            else
            {
                var path = await File.ReadAllTextAsync(cfgFilePath);
                if (path != Environment.ProcessPath)
                    await File.WriteAllTextAsync(cfgFilePath, Environment.ProcessPath);
            }

            IsValidPath = true;
            return IsValidPath;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Failed to verify MuseDash.exe\nMake sure you selected the right folder");
            await _settings.OnChoosePath();
            IsValidPath = false;
            return IsValidPath;
        }
    }

    public async Task<string> ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(_settings.Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion");
            return bundleVersion!.AsString;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox(
                $"Cannot read current game version\nDo you fully installed Muse Dash?\nPlease check your globalgamemanagers file in\n{bundlePath}");
            Environment.Exit(0);
        }

        return string.Empty;
    }

    public async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(_settings.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await _dialogueService.CreateConfirmMessageBox("Notice",
            "You did not install MelonLoader\nWhich is needed to run all the mods\nInstall Now?");
        if (install)
            await OnInstallMelonLoader();
    }

    public async Task OnInstallMelonLoader()
    {
        if (!IsValidPath) return;
        await DialogHost.Show(_downloadWindowViewModel, "DownloadWindowDialog",
            (object _, DialogOpenedEventArgs _) => _downloadWindowViewModel.InstallMelonLoader());
    }

    public async Task OnUninstallMelonLoader()
    {
        if (!IsValidPath) return;
        var result = await _dialogueService.CreateConfirmMessageBox(
            "You are asking to uninstall MelonLoader\nPlease confirm your operation");
        if (!result) return;
        var melonLoaderFolder = Path.Join(_settings.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settings.Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(_settings.Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(melonLoaderFolder))
            try
            {
                Directory.Delete(melonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                await _dialogueService.CreateMessageBox("Success", "MelonLoader has been successfully uninstalled\n");
            }
            catch (Exception)
            {
                await _dialogueService.CreateErrorMessageBox("Cannot uninstall MelonLoader\nPlease make sure your game is not running!");
            }
        else
            await _dialogueService.CreateErrorMessageBox("Cannot find MelonLoader Folder\nHave you installed MelonLoader?");
    }

    public async Task OpenModsFolder()
    {
        if (!IsValidPath)
        {
            await _dialogueService.CreateErrorMessageBox("Choose correct Muse Dash folder first!");
            await _settings.OnChoosePath();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _settings.Settings.ModsFolder,
            UseShellExecute = true
        });
    }
}