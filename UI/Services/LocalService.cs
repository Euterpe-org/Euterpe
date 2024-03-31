using System.Diagnostics;
using System.Reflection;
using System.Text;
using AssetsTools.NET.Extra;
using CliWrap;
using DialogHostAvalonia;
using MelonLoader;

namespace MuseDashModToolsUI.Services;

#pragma warning disable CS8618

public sealed partial class LocalService : ILocalService
{
    private bool IsValidPath { get; set; }

    public async Task CheckDotNetRuntimeInstallAsync()
    {
        var outputStringBuilder = new StringBuilder();
        await Cli.Wrap("dotnet")
            .WithArguments("--list-runtimes")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputStringBuilder))
            .ExecuteAsync();

        if (!outputStringBuilder.ToString().Contains("Microsoft.WindowsDesktop.App 6."))
        {
            Logger.Information("DotNet Runtime not found, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_DotNetRuntimeNotFound);
        }
    }

    public async Task CheckMelonLoaderInstallAsync()
    {
        var melonLoaderFolder = Path.Join(Settings.MuseDashFolder, "MelonLoader");
        var versionFile = Path.Join(Settings.MuseDashFolder, "version.dll");
        if (Directory.Exists(melonLoaderFolder) && File.Exists(versionFile))
        {
            return;
        }

        var install = await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_InstallMelonLoader);
        if (install)
        {
            await OnInstallMelonLoaderAsync();
        }
    }

    public async Task CheckValidPathAsync()
    {
        Logger.Information("Checking valid path...");
        await CheckGameFileExist();

        try
        {
            if (!await PlatformService.VerifyGameVersionAsync())
            {
                return;
            }

            await CreateFiles();
            IsValidPath = true;
            Logger.Information("Path verified {Path}", Settings.MuseDashFolder);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exe verify failed, showing error message box...");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_ExeVerifyFailed);
        }
    }

    public IEnumerable<string> GetModFiles(string path) => Directory.GetFiles(path)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll");

    public string[] GetBmsFiles(string path) => Directory.GetFiles(path, "*.bms");

    public async Task<bool> LaunchUpdaterAsync(string link)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterFilePath = PlatformService.GetUpdaterFilePath(currentDirectory);
        var updaterTargetPath = PlatformService.GetUpdaterFilePath(updaterTargetFolder);

        if (!await CheckUpdaterFilesExist(updaterFilePath, updaterTargetFolder))
        {
            return false;
        }

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
        if (attribute is null)
        {
            return null;
        }

        mod.Name = attribute.Name;
        mod.LocalVersion = attribute.Version;

        if (mod.Name == null || mod.LocalVersion == null)
        {
            return null;
        }

        mod.Author = attribute.Author;
        mod.HomePage = attribute.DownloadLink;
        mod.SHA256 = MelonUtils.ComputeSimpleSHA256Hash(filePath);
        Logger.Information("Local mod {Name} loaded. File name {FileName}", mod.Name, mod.FileName);
        return mod;
    }

    public async Task OnInstallMelonLoaderAsync()
    {
        if (!IsValidPath)
        {
            return;
        }

        Logger.Information("Showing MelonLoader download window...");
        await DialogHost.Show(DownloadWindowViewModel.Value, "DownloadWindowDialog",
            (object _, DialogOpenedEventArgs _) => DownloadWindowViewModel.Value.InstallMelonLoader());
    }

    public async Task OnUninstallMelonLoaderAsync()
    {
        if (!IsValidPath)
        {
            return;
        }

        if (!await MessageBoxService.WarningConfirmMessageBox(MsgBox_Content_UninstallMelonLoader))
        {
            return;
        }

        var versionFile = Path.Join(Settings.MuseDashFolder, "version.dll");
        var noticeTxt = Path.Join(Settings.MuseDashFolder, "NOTICE.txt");

        if (Directory.Exists(Settings.MelonLoaderFolder))
        {
            try
            {
                Directory.Delete(Settings.MelonLoaderFolder, true);
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

    public void OnLaunchGame(bool isModded)
    {
        using var launchArguments = ZString.CreateStringBuilder(true);
        if (!isModded)
        {
            launchArguments.Append("//--no-mods");
        }
        else if (!Settings.ShowConsole)
        {
            launchArguments.Append("//--melonloader.hideconsole");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = $"steam://rungameid/774171{launchArguments}",
            UseShellExecute = true
        });

        Logger.Information("Launching game with launch arguments: {LaunchArguments}", launchArguments);
    }

    public async Task OpenCustomAlbumsFolderAsync()
    {
        if (!await ValidatePathAsync())
        {
            return;
        }

        Logger.Information("Opening Custom Albums folder...");
        PlatformService.OpenFolder(Settings.CustomAlbumsFolder);
    }

    public async Task OpenModsFolderAsync()
    {
        if (!await ValidatePathAsync())
        {
            return;
        }

        Logger.Information("Opening mods folder...");
        PlatformService.OpenFolder(Settings.ModsFolder);
    }

    public async Task OpenUserDataFolderAsync()
    {
        if (!await ValidatePathAsync())
        {
            return;
        }

        Logger.Information("Opening UserData folder...");
        PlatformService.OpenFolder(Settings.UserDataFolder);
    }

    public async Task OpenLogFileAsync()
    {
        if (!await ValidatePathAsync())
        {
            return;
        }

        var logPath = Path.Combine(Settings.MelonLoaderFolder, "Latest.log");
        Logger.Information("Opening Log file...");
        PlatformService.OpenFile(logPath);
    }

    public async ValueTask<string> ReadGameVersionAsync()
    {
        var assetsManager = new AssetsManager();
        var bundlePath = Path.Join(Settings.MuseDashFolder, "MuseDash_Data", "globalgamemanagers");
        try
        {
            var instance = assetsManager.LoadAssetsFile(bundlePath, true);
            assetsManager.LoadIncludedClassPackage();
            if (!instance.file.Metadata.TypeTreeEnabled)
            {
                assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);
            }

            var playerSettings = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings)[0];

            var bundleVersion = assetsManager.GetBaseField(instance, playerSettings)?.Get("bundleVersion").AsString!;
            Logger.Information("Game version read successfully: {BundleVersion}", bundleVersion);
            return bundleVersion;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Read game version failed, showing error message box...");
            await MessageBoxService.FormatErrorMessageBox(MsgBox_Content_ReadGameVersionFailed, bundlePath);
            Environment.Exit(0);
        }

        return string.Empty;
    }

    #region Services

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public Lazy<IDownloadWindowViewModel> DownloadWindowViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISavingService> SavingService { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    #endregion
}