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

public class LocalService : ILocalService
{
    private readonly IDialogueService _dialogueService;
    private readonly IDownloadWindowViewModel _downloadWindowViewModel;
    private readonly ILogger _logger;
    private readonly ISettingService _settingService;

    private bool IsValidPath { get; set; }

    public LocalService(IDialogueService dialogueService, IDownloadWindowViewModel downloadWindowViewModel, ILogger logger,
        ISettingService settingService
    )
    {
        _dialogueService = dialogueService;
        _downloadWindowViewModel = downloadWindowViewModel;
        _logger = logger;
        _settingService = settingService;
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
        var exePath = Path.Join(_settingService.Settings.MuseDashFolder, "MuseDash.exe");
        var gameAssemblyPath = Path.Join(_settingService.Settings.MuseDashFolder, "GameAssembly.dll");
        var userDataPath = Path.Join(_settingService.Settings.MuseDashFolder, "UserData");
        if (!File.Exists(exePath) || !File.Exists(gameAssemblyPath))
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_NoExeFound.Localize());
            await _settingService.OnChoosePath();
        }

        try
        {
            var version = FileVersionInfo.GetVersionInfo(exePath).FileVersion;
            if (version is not "2019.4.32.16288752")
            {
                await _dialogueService.CreateErrorMessageBox(MsgBox_Content_IncorrectVersion.Localize());
                IsValidPath = false;
                return IsValidPath;
            }

            if (!Directory.Exists(_settingService.Settings.ModsFolder))
                Directory.CreateDirectory(_settingService.Settings.ModsFolder);

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            var cfgFilePath = Path.Join(_settingService.Settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
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
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_ExeVerifyFailed.Localize());
            await _settingService.OnChoosePath();
            IsValidPath = false;
            return IsValidPath;
        }
    }

    public async Task<string> ReadGameVersion()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(_settingService.Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
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
            await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_ReadGameVersionFailed.Localize(), bundlePath));
            Environment.Exit(0);
        }

        return string.Empty;
    }

    public async Task CheckMelonLoaderInstall()
    {
        var melonLoaderFolder = Path.Join(_settingService.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settingService.Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile)) return;
        var install = await _dialogueService.CreateConfirmMessageBox(MsgBox_Title_Notice, MsgBox_Content_InstallMelonLoader.Localize());
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
        var result = await _dialogueService.CreateConfirmMessageBox(MsgBox_Content_UninstallMelonLoader.Localize());
        if (!result) return;
        var melonLoaderFolder = Path.Join(_settingService.Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(_settingService.Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(_settingService.Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(melonLoaderFolder))
            try
            {
                Directory.Delete(melonLoaderFolder, true);
                File.Delete(versionFile);
                File.Delete(noticeTxt);
                await _dialogueService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_UninstallMelonLoaderSuccess.Localize());
            }
            catch (Exception)
            {
                await _dialogueService.CreateErrorMessageBox(MsgBox_Content_UninstallMelonLoaderFailed.Localize());
            }
        else
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_NoMelonLoaderFolder.Localize());
    }

    public async Task OpenModsFolder()
    {
        if (!IsValidPath)
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await _settingService.OnChoosePath();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _settingService.Settings.ModsFolder,
            UseShellExecute = true
        });
    }

    public async Task OpenUserDataFolder()
    {
        if (!IsValidPath)
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
            await _settingService.OnChoosePath();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _settingService.Settings.UserDataFolder,
            UseShellExecute = true
        });
    }
}