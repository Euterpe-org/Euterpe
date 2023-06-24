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
using MessageBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class ModService : IModService
{
    private string? _currentGameVersion;

    private ReadOnlyObservableCollection<Mod>? _mods;
    private SourceCache<Mod?, string>? _sourceCache;
    public IDialogueService? DialogueService { get; init; }
    public IGitHubService? GitHubService { get; init; }
    public ILocalService? LocalService { get; init; }
    public ILogger? Logger { get; init; }
    public ISettingService? Settings { get; init; }

    public async Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods)
    {
        Logger?.Information("Initializing mod list...");
        _sourceCache = sourceCache!;
        _mods = mods;
        var isValidPath = await LocalService!.CheckValidPath();
        if (!isValidPath) return;
        _currentGameVersion = await LocalService!.ReadGameVersion();
        await LocalService!.CheckMelonLoaderInstall();

        var webMods = await GitHubService!.GetModsAsync();
        var localPaths = LocalService!.GetModFiles(Settings!.Settings.ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(LocalService!.LoadMod).Where(mod => mod is not null).ToList()!;
            Logger?.Information("Read all local mods info success");
        }
        catch (Exception ex)
        {
            await DialogueService!.CreateErrorMessageBox(MsgBox_Content_BrokenMods.Localize());
            await LocalService!.OpenModsFolder();
            Logger?.Fatal(ex, "Load local mods failed");
            Environment.Exit(0);
            return;
        }

        await LoadModsToUI(localMods, webMods!);
    }

    public async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            Logger?.Error("Download link is null");
            await DialogueService!.CreateErrorMessageBox(MsgBox_Content_NoDownloadLink.Localize());
            return;
        }

        var errors = new StringBuilder();

        try
        {
            var path = Path.Join(Settings!.Settings.ModsFolder, item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await GitHubService!.DownloadModAsync(item.DownloadLink, path);
            var downloadedMod = LocalService!.LoadMod(path)!;
            var webMods = await GitHubService!.GetModsAsync();
            var mod = webMods?.FirstOrDefault(x => x.Name == downloadedMod.Name)!;
            mod.IsDisabled = downloadedMod.IsDisabled;
            mod.FileName = downloadedMod.FileName;
            mod.LocalVersion = downloadedMod.LocalVersion;
            Logger?.Information("Install mod {Name} success", mod.Name);
            _sourceCache?.AddOrUpdate(mod);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case HttpRequestException:
                    errors.AppendLine(string.Format(MsgBox_Content_InstallModFailed_Internet.Localize(), ex));
                    break;

                case SecurityException:
                case UnauthorizedAccessException:
                case IOException:
                    errors.AppendLine(string.Format(MsgBox_Content_InstallModFailed_Game.Localize(), ex));
                    break;

                default:
                    errors.AppendLine(string.Format(MsgBox_Content_InstallModFailed.Localize(), ex));
                    break;
            }
        }

        var dependencies = SearchDependencies(item.Name!).ToArray();
        foreach (var dependency in dependencies)
        {
            var installedMod = _mods!.FirstOrDefault(x => x.Name == dependency.Name && x.IsLocal);
            if (installedMod is not null) continue;
            try
            {
                var path = Path.Join(Settings!.Settings.ModsFolder, dependency.DownloadLink!.Split("/")[1]);
                await GitHubService!.DownloadModAsync(dependency.DownloadLink, path);
                var mod = LocalService!.LoadMod(path);
                Logger?.Information("Install dependency {Name} success", mod!.Name);
                _sourceCache!.AddOrUpdate(mod);
            }
            catch (Exception ex)
            {
                errors.AppendLine(string.Format(MsgBox_Content_InstallDependencyFailed.Localize(), ex));
            }
        }

        var disabledDependencies = dependencies.Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
        if (disabledDependencies.Length > 0)
        {
            var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x?.Name));
            Settings!.Settings.AskEnableDependenciesWhenInstalling = await ChangeDependenciesState(
                string.Format(MsgBox_Content_EnableDependency, item.Name, disabledDependencyNames),
                disabledDependencies, Settings!.Settings.AskEnableDependenciesWhenInstalling, false);
        }

        if (errors.Length > 0)
        {
            Logger?.Error("Install mod {Name} failed: {Errors}", item.Name, errors.ToString());
            await DialogueService!.CreateErrorMessageBox(errors.ToString());
            return;
        }

        await DialogueService!.CreateMessageBox(MsgBox_Title_Success,
            string.Format(MsgBox_Content_InstallModSuccess.Localize(), item.Name));
    }

    public async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            Logger?.Information("Updating mod {Name}", item.Name);
            await OnInstallMod(item);
            return;
        }

        var result = await DialogueService!.CreateConfirmMessageBox(
            string.Format(MsgBox_Content_ReinstallMod.Localize(), item.Name));
        if (!result) return;
        Logger?.Information("Reinstalling mod {Name}", item.Name);
        await OnInstallMod(item);
    }

    public async Task OnToggleMod(Mod? item)
    {
        try
        {
            switch (item!.IsDisabled)
            {
                case true:
                    var enabledReverseDependencies = SearchReverseDependencies(item.Name!)
                        .Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
                    if (enabledReverseDependencies.Length > 0)
                    {
                        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                        var result = await DialogueService!.CreateConfirmMessageBox(
                            string.Format(MsgBox_Content_DisableModConfirm.Localize(), item.Name, enabledReverseDependencyNames));
                        if (!result)
                        {
                            item.IsDisabled = !item.IsDisabled;
                            return;
                        }

                        Settings!.Settings.AskDisableDependenciesWhenDisabling = await ChangeDependenciesState(
                            string.Format(MsgBox_Content_DisableReverseDependency, item.Name), enabledReverseDependencies,
                            Settings!.Settings.AskDisableDependenciesWhenDisabling, true);
                    }

                    break;
                case false:
                    var disabledDependencies = SearchDependencies(item.Name!)
                        .Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
                    if (disabledDependencies.Length > 0)
                    {
                        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
                        Settings!.Settings.AskEnableDependenciesWhenEnabling = await ChangeDependenciesState(
                            string.Format(MsgBox_Content_EnableDependency, item.Name, disabledDependencyNames),
                            disabledDependencies, Settings!.Settings.AskEnableDependenciesWhenEnabling, false);
                    }

                    break;
            }

            File.Move(
                Path.Join(Settings!.Settings.ModsFolder, item.FileNameExtended(true)),
                Path.Join(Settings!.Settings.ModsFolder, item.FileNameExtended()));
            Logger?.Information("Change mod {Name} state to {State}", item.Name, item.IsDisabled ? "Disabled" : "Enabled");
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    Logger?.Error(ex, "Change mod {Name} state failed", item?.Name);
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed_Unauthorized.Localize(),
                        ex));
                    break;

                case IOException:
                    Logger?.Error(ex, "Change mod {Name} state failed", item?.Name);
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed_Game.Localize(), ex));
                    break;

                default:
                    Logger?.Error(ex, "Change mod {Name} state failed", item?.Name);
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed.Localize(), ex));
                    break;
            }

            item!.IsDisabled = !item.IsDisabled;
        }
    }

    public async Task OnDeleteMod(Mod item)
    {
        if (item.IsDuplicated)
        {
            await DialogueService!.CreateMessageBox(MsgBox_Title_Notice,
                string.Format(MsgBox_Content_DuplicateMods.Localize(), item.DuplicatedModNames), icon: Icon.Info);
            await LocalService!.OpenModsFolder();
            return;
        }

        var path = Path.Join(Settings!.Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            Logger?.Error("Delete mod {Name} failed: File not found", item.Name);
            await DialogueService!.CreateErrorMessageBox(MsgBox_Content_UninstallModFailed_Null);
            return;
        }

        try
        {
            var enabledReverseDependencies = SearchReverseDependencies(item.Name!)
                .Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
            if (enabledReverseDependencies.Length > 0)
            {
                var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x?.Name));
                var result = await DialogueService!.CreateConfirmMessageBox(
                    string.Format(MsgBox_Content_DeleteModConfirm, item.Name, enabledReverseDependencyNames));
                if (!result) return;
                Settings!.Settings.AskDisableDependenciesWhenDeleting = await ChangeDependenciesState(
                    string.Format(MsgBox_Content_DisableReverseDependency, item.Name), enabledReverseDependencies,
                    Settings!.Settings.AskDisableDependenciesWhenDeleting, true);
            }

            File.Delete(path);
            _sourceCache!.Remove(item);
            Logger?.Information("Delete mod {Name} success", item.Name);
            var webMods = await GitHubService!.GetModsAsync();
            var webMod = webMods?.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache?.AddOrUpdate(webMod);
                Logger?.Information("Update deleted mod info success");
            }

            await DialogueService!.CreateMessageBox(MsgBox_Title_Success,
                string.Format(MsgBox_Content_UninstallModSuccess.Localize(), item.Name));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    Logger?.Error(ex, "Delete mod {Name} failed", item.Name);
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_UninstallModFailed_Game.Localize(), ex));
                    break;

                default:
                    Logger?.Error(ex, "Delete mod {Name} failed", item.Name);
                    await DialogueService!.CreateErrorMessageBox(string.Format(MsgBox_Content_UninstallModFailed.Localize(), ex));
                    break;
            }
        }
    }

    private async Task LoadModsToUI(List<Mod> localMods, List<Mod> webMods)
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
                    string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
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
                    string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
                Logger?.Information("Found duplicated mod {DuplicateMods}", localMod.DuplicatedModNames);
            }

            _sourceCache?.AddOrUpdate(localMods[i]);
        }
    }

    private async Task CheckModToolsInstall(Mod mod)
    {
        if (Settings!.Settings.AskInstallMuseDashModTools != AskType.Always) return;
        if (mod.Name != "MuseDashModTools") return;
        var result = await DialogueService!.CreateCustomConfirmMessageBox(MsgBox_Content_InstallModTools.Localize(), 3);
        if (result == MsgBox_Button_Yes) await OnInstallMod(mod);
        else if (result == MsgBox_Button_NoNoAsk) Settings!.Settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
    }

    private IEnumerable<Mod> SearchDependencies(string modName)
    {
        var dependencyNames = _sourceCache?.Lookup(modName).Value?.DependencyNames.Split("\r\n");
        Logger?.Information("Search dependencies of {ModName}: {DependencyNames}", modName, dependencyNames);
        return dependencyNames?.Where(x => _sourceCache!.Lookup(x).HasValue)
            .Select(x => _sourceCache!.Lookup(x).Value)!;
    }

    private IEnumerable<Mod> SearchReverseDependencies(string modName)
    {
        var reverseDependencyNames = _sourceCache?.Items.Where(x => x!.DependencyNames.Split("\r\n").Contains(modName))
            .Select(x => x?.Name).ToArray();
        Logger?.Information("Search reverse dependencies of {ModName}: {ReverseDependencyNames}", modName, reverseDependencyNames);
        return _sourceCache?.Items.Where(x => reverseDependencyNames!.Contains(x?.Name))!;
    }

    private async Task<AskType> ChangeDependenciesState(string content, IEnumerable<Mod?> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await DialogueService!.CreateCustomConfirmMessageBox(content, 4);
                if (askResult == MsgBox_Button_Yes)
                {
                    await ChangeState();
                }
                else if (askResult == MsgBox_Button_YesNoAsk)
                {
                    await ChangeState();
                    askType = AskType.YesAndNoAsk;
                }
                else if (askResult == MsgBox_Button_NoNoAsk)
                {
                    askType = AskType.NoAndNoAsk;
                }

                break;
            case AskType.YesAndNoAsk:
                await ChangeState();
                break;
            case AskType.NoAndNoAsk:
            default: break;
        }

        async Task ChangeState()
        {
            foreach (var dependency in dependencies)
            {
                dependency!.IsDisabled = turnOff;
                await OnToggleMod(dependency);
            }
        }

        return askType;
    }

    private bool CheckCompatible(Mod mod) =>
        mod.CompatibleGameVersion == XAML_Mod_CompatibleGameVersion || mod.GameVersion!.Contains(_currentGameVersion);
}