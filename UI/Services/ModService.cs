using System.Collections.ObjectModel;
using DynamicData;
using NuGet.Versioning;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public sealed partial class ModService : IModService
{
    private string _currentGameVersion;
    private bool[] _isTracked;
    private List<Mod> _localMods;
    private ReadOnlyObservableCollection<Mod> _mods;
    private SourceCache<Mod, string> _sourceCache;
    private List<Mod>? _webMods;

    [UsedImplicitly]
    public IDownloadService DownloadService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    [UsedImplicitly]
    public ISettingsViewModel SettingsViewModel { get; init; }

    public bool CompareVersion(string modName, string modVersion)
    {
        var webMod = _webMods?.Find(x => x.Name == modName);
        if (webMod is null)
        {
            return false;
        }

        var webModVersion = SemanticVersion.Parse(webMod.Version);
        var loadedModVersion = SemanticVersion.Parse(modVersion);

        return webModVersion > loadedModVersion;
    }

    public async Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods)
    {
        Logger.Information("Initializing mod list...");
        _sourceCache = sourceCache;
        _mods = mods;
        _currentGameVersion = await LocalService.ReadGameVersionAsync();
        await LocalService.CheckDotNetRuntimeInstallAsync();
        await LocalService.CheckMelonLoaderInstallAsync();

        _webMods ??= await DownloadService.GetModListAsync();
        if (_webMods is null)
        {
            return;
        }

        var localPaths = LocalService.GetModFiles(Settings.ModsFolder);
        try
        {
            _localMods = localPaths.Select(LocalService.LoadMod).Where(mod => mod is not null).ToList()!;
            Logger.Information("Read all local mods info success");
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_BrokenMods, ex);
            await LocalService.OpenModsFolderAsync();
            Logger.Fatal(ex, "Load local mods failed");
            Environment.Exit(0);
            return;
        }

        await LoadModsToUI();
    }

    public async Task OnInstallModAsync(Mod item)
    {
        if (!string.IsNullOrEmpty(item.DownloadLink))
        {
            Logger.Error("Download link of mod {Name} is empty", item.Name);
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoDownloadLink);
            return;
        }

        using var errors = ZString.CreateStringBuilder(true);

        try
        {
            var path = Path.Join(Settings.ModsFolder,
                item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await DownloadService.DownloadModAsync(item.DownloadLink, path);

            var downloadedMod = LocalService.LoadMod(path)!;
            _webMods ??= await DownloadService.GetModListAsync();
            if (_webMods is null)
            {
                return;
            }

            var webMod = _webMods.Find(x => x.Name == downloadedMod.Name)!;
            downloadedMod.CloneOnlineInfo(webMod);
            CheckConfigFileExist(webMod);
            CheckVersionState(webMod, downloadedMod);

            Logger.Information("Install mod {Name} success", downloadedMod.Name);
            _sourceCache.AddOrUpdate(downloadedMod);
        }
        catch (Exception ex)
        {
            HandleInstallModException(ex, errors);
        }

        errors.Append(await CheckDependencyInstall(item));

        if (errors.Length > 0)
        {
            Logger.Error("Install mod {Name} failed: {Errors}", item.Name, errors.ToString());
            await MessageBoxService.ErrorMessageBox(errors.ToString());
            return;
        }

        await MessageBoxService.FormatSuccessMessageBox(MsgBox_Content_InstallModSuccess, item.Name);
    }

    public async Task OnReinstallModAsync(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            Logger.Information("Updating mod {Name}", item.Name);
            await OnInstallModAsync(item);
            return;
        }

        var reinstall = await MessageBoxService.FormatWarningConfirmMessageBox(MsgBox_Content_ReinstallMod, item.Name);
        if (!reinstall)
        {
            return;
        }

        Logger.Information("Reinstalling mod {Name}", item.Name);
        File.Delete(Path.Join(Settings.ModsFolder, item.FileNameExtended()));
        await OnInstallModAsync(item);
    }

    public async Task OnToggleModAsync(Mod item)
    {
        try
        {
            if (item.IsDisabled)
            {
                var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DisableModConfirm,
                    Settings.AskDisableDependencyWhenDisable);
                if (!result)
                {
                    return;
                }

                SettingsViewModel.DisableDependenciesWhenDisabling = (int)askType;
            }
            else
            {
                await CheckDependencyInstall(item);
            }

            File.Move(Path.Join(Settings.ModsFolder, item.FileNameExtended(true)),
                Path.Join(Settings.ModsFolder, item.FileNameExtended()));
            Logger.Information("Change mod {Name} state to {State}", item.Name,
                item.IsDisabled ? "Disabled" : "Enabled");
        }
        catch (Exception ex)
        {
            await HandleToggleModException(item, ex);
        }
    }

    public async Task OnDeleteModAsync(Mod item)
    {
        if (item.IsDuplicated)
        {
            await MessageBoxService.FormatNoticeMessageBox(MsgBox_Content_DuplicateMods, item.DuplicatedModNames);
            await LocalService.OpenModsFolderAsync();
            return;
        }

        var path = Path.Join(Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            Logger.Error("Delete mod {Name} failed: File not found", item.Name);
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_UninstallModFailed_Null);
            return;
        }

        try
        {
            var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DeleteModConfirm,
                Settings.AskDisableDependencyWhenDelete);
            if (!result)
            {
                return;
            }

            SettingsViewModel.DisableDependenciesWhenDeleting = (int)askType;
            File.Delete(path);
            var mod = _webMods?.Find(x => x.Name == item.Name)?.RemoveLocalInfo();
            _sourceCache!.AddOrUpdate(mod);
            Logger.Information("Delete mod {Name} success", item.Name);
            await MessageBoxService.FormatSuccessMessageBox(MsgBox_Content_UninstallModSuccess, item.Name);
        }
        catch (Exception ex)
        {
            await HandleDeleteModException(item, ex);
        }
    }
}