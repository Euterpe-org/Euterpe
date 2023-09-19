using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DynamicData;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using NuGet.Versioning;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class ModService : IModService
{
    private string? _currentGameVersion;

    private ReadOnlyObservableCollection<Mod>? _mods;
    private SourceCache<Mod?, string>? _sourceCache;
    private List<Mod>? _webMods;

    public IGitHubService GitHubService { get; init; }
    public ILocalService LocalService { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISavingService SavingService { get; init; }
    public ISettingsViewModel SettingsViewModel { get; init; }

    public bool CompareVersion(string modName, string modVersion)
    {
        var webMod = _webMods?.Find(x => x.Name == modName);
        if (webMod is null) return false;

        var webModVersion = SemanticVersion.Parse(webMod.Version!);
        var loadedModVersion = SemanticVersion.Parse(modVersion);

        return webModVersion > loadedModVersion;
    }

    public async Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods)
    {
        Logger.Information("Initializing mod list...");
        _sourceCache = sourceCache!;
        _mods = mods;
        _currentGameVersion = await LocalService.ReadGameVersion();
        await LocalService.CheckMelonLoaderInstall();

        _webMods ??= await GitHubService.GetModListAsync();
        if (_webMods is null) return;
        var localPaths = LocalService.GetModFiles(SavingService.Settings.ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(LocalService.LoadMod).Where(mod => mod is not null).ToList()!;
            Logger.Information("Read all local mods info success");
        }
        catch (Exception ex)
        {
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_BrokenMods, ex);
            await LocalService.OpenModsFolder();
            Logger.Fatal(ex, "Load local mods failed");
            Environment.Exit(0);
            return;
        }

        await LoadModsToUI(localMods, _webMods);
    }

    public async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            Logger.Error("Download link is null");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoDownloadLink);
            return;
        }

        var errors = new StringBuilder();

        try
        {
            var path = Path.Join(SavingService.Settings.ModsFolder,
                item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await GitHubService.DownloadModAsync(item.DownloadLink, path);

            var downloadedMod = LocalService.LoadMod(path)!;
            _webMods ??= await GitHubService.GetModListAsync();
            if (_webMods is null) return;
            var webMod = _webMods.Find(x => x.Name == downloadedMod.Name)!;
            downloadedMod.CloneOnlineInfo(webMod);
            CheckVersionState(webMod, downloadedMod);

            Logger.Information("Install mod {Name} success", downloadedMod.Name);
            _sourceCache?.AddOrUpdate(downloadedMod);
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

        await MessageBoxService.SuccessMessageBox(string.Format(MsgBox_Content_InstallModSuccess, item.Name));
    }

    public async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            Logger.Information("Updating mod {Name}", item.Name);
            await OnInstallMod(item);
            return;
        }

        var reinstall = await MessageBoxService.WarningConfirmMessageBox(string.Format(MsgBox_Content_ReinstallMod, item.Name));
        if (!reinstall) return;
        Logger.Information("Reinstalling mod {Name}", item.Name);
        File.Delete(Path.Join(SavingService.Settings.ModsFolder, item.FileNameExtended()));
        await OnInstallMod(item);
    }

    public async Task OnToggleMod(Mod item)
    {
        try
        {
            if (item.IsDisabled)
            {
                var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DisableModConfirm,
                    SavingService.Settings.AskDisableDependenciesWhenDisabling);
                if (!result)
                    return;

                SettingsViewModel.DisableDependenciesWhenDisabling = (int)askType;
            }
            else
            {
                await CheckDependencyInstall(item);
            }

            File.Move(Path.Join(SavingService.Settings.ModsFolder, item.FileNameExtended(true)),
                Path.Join(SavingService.Settings.ModsFolder, item.FileNameExtended()));
            Logger.Information("Change mod {Name} state to {State}", item.Name,
                item.IsDisabled ? "Disabled" : "Enabled");
        }
        catch (Exception ex)
        {
            await HandleToggleModException(item, ex);
        }
    }

    public async Task OnDeleteMod(Mod item)
    {
        if (item.IsDuplicated)
        {
            await MessageBoxService.NoticeMessageBox(string.Format(MsgBox_Content_DuplicateMods, item.DuplicatedModNames));
            await LocalService.OpenModsFolder();
            return;
        }

        var path = Path.Join(SavingService.Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            Logger.Error("Delete mod {Name} failed: File not found", item.Name);
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_UninstallModFailed_Null);
            return;
        }

        try
        {
            var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DeleteModConfirm,
                SavingService.Settings.AskDisableDependenciesWhenDeleting);
            if (!result)
                return;

            SettingsViewModel.DisableDependenciesWhenDeleting = (int)askType;
            File.Delete(path);
            var mod = _webMods?.Find(x => x.Name == item.Name)?.RemoveLocalInfo();
            _sourceCache?.AddOrUpdate(mod);
            Logger.Information("Delete mod {Name} success", item.Name);
            await MessageBoxService.SuccessMessageBox(string.Format(MsgBox_Content_UninstallModSuccess, item.Name));
        }
        catch (Exception ex)
        {
            await HandleDeleteModException(item, ex);
        }
    }
}