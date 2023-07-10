using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using MsBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public class ModService : IModService
{
    private string? _currentGameVersion;

    private ReadOnlyObservableCollection<Mod>? _mods;
    private SourceCache<Mod?, string>? _sourceCache;
    private List<Mod>? _webMods;

    public IGitHubService GitHubService { get; init; }
    public ILocalService LocalService { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISettingService SettingService { get; init; }
    public ISettingsViewModel SettingsViewModel { get; init; }

    public async Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods)
    {
        Logger.Information("Initializing mod list...");
        _sourceCache = sourceCache!;
        _mods = mods;
        var isValidPath = await LocalService.CheckValidPath();
        if (!isValidPath) return;
        _currentGameVersion = await LocalService.ReadGameVersion();
        await LocalService.CheckMelonLoaderInstall();

        _webMods ??= await GitHubService.GetModListAsync();
        if (_webMods is null) return;
        var localPaths = LocalService.GetModFiles(SettingService.Settings.ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(LocalService.LoadMod).Where(mod => mod is not null).ToList()!;
            Logger.Information("Read all local mods info success");
        }
        catch (Exception ex)
        {
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_BrokenMods.Localize(), ex));
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
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoDownloadLink.Localize());
            return;
        }

        var errors = new StringBuilder();

        try
        {
            var path = Path.Join(SettingService.Settings.ModsFolder,
                item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await GitHubService.DownloadModAsync(item.DownloadLink, path);
            var downloadedMod = LocalService.LoadMod(path)!;
            _webMods ??= await GitHubService.GetModListAsync();
            if (_webMods is null) return;
            var mod = _webMods?.FirstOrDefault(x => x.Name == downloadedMod.Name)!;
            mod.IsDisabled = downloadedMod.IsDisabled;
            mod.FileName = downloadedMod.FileName;
            mod.LocalVersion = downloadedMod.LocalVersion;
            mod.SHA256 = downloadedMod.SHA256;
            Logger.Information("Install mod {Name} success", mod.Name);
            _sourceCache?.AddOrUpdate(mod);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case HttpRequestException:
                    errors.AppendFormat(MsgBox_Content_InstallModFailed_Internet.Localize(), ex).AppendLine();
                    break;

                case SecurityException:
                case UnauthorizedAccessException:
                case IOException:
                    errors.AppendFormat(MsgBox_Content_InstallModFailed_Game.Localize(), ex).AppendLine();
                    break;

                default:
                    errors.AppendFormat(MsgBox_Content_InstallModFailed.Localize(), ex).AppendLine();
                    break;
            }
        }

        await CheckDependencyInstall(item);

        if (errors.Length > 0)
        {
            Logger.Error("Install mod {Name} failed: {Errors}", item.Name, errors.ToString());
            await MessageBoxService.CreateErrorMessageBox(errors.ToString());
            return;
        }

        await MessageBoxService.CreateSuccessMessageBox(string.Format(MsgBox_Content_InstallModSuccess.Localize(), item.Name));
    }

    public async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            Logger.Information("Updating mod {Name}", item.Name);
            await OnInstallMod(item);
            return;
        }

        var reinstall = await MessageBoxService.CreateConfirmMessageBox(string.Format(MsgBox_Content_ReinstallMod.Localize(), item.Name));
        if (!reinstall) return;
        Logger.Information("Reinstalling mod {Name}", item.Name);
        await OnInstallMod(item);
    }

    public async Task OnToggleMod(Mod item)
    {
        try
        {
            if (item.IsDisabled)
            {
                var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DisableModConfirm.Localize(),
                    SettingService.Settings.AskDisableDependenciesWhenDisabling);
                if (!result)
                    return;

                SettingsViewModel.DisableDependenciesWhenDisabling = (int)askType;
            }
            else
            {
                await CheckDependencyInstall(item);
            }

            File.Move(Path.Join(SettingService.Settings.ModsFolder, item.FileNameExtended(true)),
                Path.Join(SettingService.Settings.ModsFolder, item.FileNameExtended()));
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
            await MessageBoxService.CreateMessageBox(MsgBox_Title_Notice,
                string.Format(MsgBox_Content_DuplicateMods.Localize(), item.DuplicatedModNames), icon: Icon.Info);
            await LocalService.OpenModsFolder();
            return;
        }

        var path = Path.Join(SettingService.Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            Logger.Error("Delete mod {Name} failed: File not found", item.Name);
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_UninstallModFailed_Null);
            return;
        }

        try
        {
            var (result, askType) = await DisableReverseDependencies(item, MsgBox_Content_DeleteModConfirm.Localize(),
                SettingService.Settings.AskDisableDependenciesWhenDeleting);
            if (!result)
                return;

            SettingsViewModel.DisableDependenciesWhenDeleting = (int)askType;
            File.Delete(path);
            var mod = _webMods?.FirstOrDefault(x => x.Name == item.Name)?.SetDefault();
            _sourceCache?.AddOrUpdate(mod);
            Logger.Information("Delete mod {Name} success", item.Name);
            await MessageBoxService.CreateSuccessMessageBox(string.Format(MsgBox_Content_UninstallModSuccess.Localize(), item.Name));
        }
        catch (Exception ex)
        {
            await HandleDeleteModException(item, ex);
        }
    }

    private async Task CheckDependencyInstall(Mod item)
    {
        var dependencies = SearchDependencies(item.Name!).ToArray();
        foreach (var dependency in dependencies)
        {
            var installedMod = _mods!.FirstOrDefault(x => x.Name == dependency.Name && x.IsLocal);
            if (installedMod is not null) continue;
            try
            {
                var path = Path.Join(SettingService.Settings.ModsFolder, dependency.DownloadLink!.Split("/")[1]);
                await GitHubService.DownloadModAsync(dependency.DownloadLink, path);
                var mod = LocalService.LoadMod(path);
                dependency.IsDisabled = mod!.IsDisabled;
                dependency.FileName = mod.FileName;
                dependency.LocalVersion = mod.LocalVersion;
                dependency.SHA256 = mod.SHA256;
                Logger.Information("Install dependency {Name} success", mod.Name);
                _sourceCache!.AddOrUpdate(dependency);
                await CheckDependencyInstall(dependency);
            }
            catch (Exception ex)
            {
                Logger.Information(ex, "Install dependency {Name} failed", dependency.Name);
            }
        }

        SettingsViewModel.EnableDependenciesWhenInstalling = (int)await EnableDependencies(item, dependencies,
            MsgBox_Content_EnableDependency, SettingService.Settings.AskEnableDependenciesWhenInstalling);
    }

    private async Task<AskType> EnableDependencies(Mod item, IEnumerable<Mod> dependencies, string message, AskType askType)
    {
        var disabledDependencies = dependencies.Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
        if (disabledDependencies.Length == 0) return askType;
        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));

        return await ChangeDependenciesState(string.Format(message, item.Name, disabledDependencyNames),
            disabledDependencies, askType, false);
    }

    private async Task<(bool, AskType)> DisableReverseDependencies(Mod item, string message, AskType askType)
    {
        var enabledReverseDependencies = SearchReverseDependencies(item.Name!).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
        if (enabledReverseDependencies.Length == 0) return (true, askType);
        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x?.Name));

        var result = await MessageBoxService.CreateConfirmMessageBox(string.Format(message, item.Name, enabledReverseDependencyNames));
        if (!result)
        {
            item.IsDisabled = result;
            return (result, askType);
        }

        return (true, await ChangeDependenciesState(string.Format(MsgBox_Content_DisableReverseDependency, item.Name),
            enabledReverseDependencies, askType, true));
    }

    private async Task HandleToggleModException(Mod item, Exception ex)
    {
        var errorMsg = ex switch
        {
            UnauthorizedAccessException => MsgBox_Content_ChangeModStateFailed_Unauthorized.Localize(),
            IOException => MsgBox_Content_ChangeModStateFailed_Game.Localize(),
            _ => MsgBox_Content_ChangeModStateFailed.Localize()
        };

        Logger.Error(ex, "Change mod {Name} state failed", item.Name);
        await MessageBoxService.CreateErrorMessageBox(string.Format(errorMsg, ex));

        item.IsDisabled = !item.IsDisabled;
    }

    private async Task HandleDeleteModException(Mod item, Exception ex)
    {
        var errorMsg = string.Format(
            ex is UnauthorizedAccessException or IOException
                ? MsgBox_Content_UninstallModFailed_Game.Localize()
                : MsgBox_Content_UninstallModFailed.Localize(), ex);

        Logger.Error(ex, "Delete mod {Name} failed", item.Name);
        await MessageBoxService.CreateErrorMessageBox(errorMsg);
    }

    private async Task LoadModsToUI(List<Mod> localMods, List<Mod>? webMods)
    {
        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods!)
        {
            var localMod = localMods.FirstOrDefault(x => x.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod!);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache?.AddOrUpdate(webMod);
                await CheckModToolsInstall(webMod);
                continue;
            }

            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames =
                    string.Join("\r\n",
                        localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            localMod.Version = webMod.Version;
            localMod.GameVersion = webMod.GameVersion;
            localMod.DependentLibs = webMod.DependentLibs;
            localMod.DependentMods = webMod.DependentMods;
            localMod.IncompatibleMods = webMod.IncompatibleMods;
            localMod.DownloadLink = webMod.DownloadLink;
            localMod.HomePage = webMod.HomePage;
            localMod.Description = webMod.Description;

            var versionState = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1
                : new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
            localMod.State = (UpdateState)versionState;
            localMod.IsShaMismatched = versionState == 0 && webMod.SHA256 != localMod.SHA256;
            if (localMod.IsShaMismatched)
                localMod.State = UpdateState.Modified;
            localMod.IsIncompatible = !CheckCompatible(localMod);
            _sourceCache?.AddOrUpdate(localMod);
            Logger?.Information("Mod {Name} loaded to UI", localMod.Name);
        }

        CheckDuplicatedMods(isTracked, localMods);
    }

    private void CheckDuplicatedMods(IReadOnlyList<bool> isTracked, IReadOnlyList<Mod> localMods)
    {
        for (var i = 0; i < isTracked.Count; i++)
        {
            if (isTracked[i]) continue;
            var localMod = localMods[i];
            if (localMods.FirstOrDefault(x => x.Name == localMod.Name)!.IsTracked) continue;
            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames =
                    string.Join("\r\n",
                        localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
                Logger.Information("Found duplicated mod {DuplicateMods}", localMod.DuplicatedModNames);
            }

            _sourceCache?.AddOrUpdate(localMods[i]);
        }
    }

    private async Task CheckModToolsInstall(Mod mod)
    {
        if (SettingService.Settings.AskInstallMuseDashModTools != AskType.Always) return;
        if (mod.Name != "MuseDashModTools") return;
        var result =
            await MessageBoxService.CreateCustomConfirmMessageBox(MsgBox_Content_InstallModTools.Localize(), 3);
        if (result == MsgBox_Button_Yes) await OnInstallMod(mod);
        else if (result == MsgBox_Button_NoNoAsk) SettingService.Settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
    }

    private IEnumerable<Mod> SearchDependencies(string modName)
    {
        var dependencyNames = _sourceCache?.Lookup(modName).Value?.DependencyNames.Split("\r\n");
        Logger.Information("Search dependencies of {ModName}: {DependencyNames}", modName, dependencyNames);
        return dependencyNames?.Where(x => _sourceCache!.Lookup(x).HasValue)
            .Select(x => _sourceCache!.Lookup(x).Value)!;
    }

    private IEnumerable<Mod?> SearchReverseDependencies(string modName)
    {
        var reverseDependencyNames = _sourceCache?.Items.Where(x => x!.DependencyNames.Split("\r\n").Contains(modName))
            .Select(x => x?.Name).ToArray();
        Logger.Information("Search reverse dependencies of {ModName}: {ReverseDependencyNames}", modName,
            reverseDependencyNames);
        return _sourceCache?.Items.Where(x => reverseDependencyNames!.Contains(x?.Name))!;
    }

    private async Task<AskType> ChangeDependenciesState(string content, IEnumerable<Mod?> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await MessageBoxService.CreateCustomConfirmMessageBox(content, 4);
                if (askResult == MsgBox_Button_Yes)
                {
                    await ChangeState(dependencies, turnOff);
                }
                else if (askResult == MsgBox_Button_YesNoAsk)
                {
                    await ChangeState(dependencies, turnOff);
                    askType = AskType.YesAndNoAsk;
                }
                else if (askResult == MsgBox_Button_NoNoAsk)
                {
                    askType = AskType.NoAndNoAsk;
                }

                break;
            case AskType.YesAndNoAsk:
                await ChangeState(dependencies, turnOff);
                break;
            case AskType.NoAndNoAsk:
            default: break;
        }

        return askType;
    }

    private async Task ChangeState(IEnumerable<Mod?> dependencies, bool turnOff)
    {
        foreach (var dependency in dependencies)
        {
            if (dependency!.IsDisabled == turnOff) continue;
            dependency.IsDisabled = turnOff;
            await OnToggleMod(dependency);
        }
    }

    private bool CheckCompatible(Mod mod) =>
        mod.CompatibleGameVersion == XAML_Mod_CompatibleGameVersion || mod.GameVersion!.Contains(_currentGameVersion);
}