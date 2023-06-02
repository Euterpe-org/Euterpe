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
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class ModService : IModService
{
    private readonly IDialogueService _dialogueService;
    private readonly IGitHubService _gitHubService;
    private readonly ILocalService _localService;
    private readonly ISettingService _settings;
    private string? _currentGameVersion;

    private SourceCache<Mod, string>? _sourceCache;
    private ReadOnlyObservableCollection<Mod>? Mods;

    public ModService(IDialogueService dialogueService, IGitHubService gitHubService, ISettingService settings, ILocalService localService)
    {
        _dialogueService = dialogueService;
        _gitHubService = gitHubService;
        _settings = settings;
        _localService = localService;
    }

    public async Task InitializeModList(SourceCache<Mod, string> sourceCache, ReadOnlyObservableCollection<Mod> mods)
    {
        _sourceCache = sourceCache;
        Mods = mods;
        var isValidPath = await _localService.CheckValidPath();
        if (!isValidPath) return;
        _currentGameVersion = await _localService.ReadGameVersion();
        await _localService.CheckMelonLoaderInstall();

        var webMods = await _gitHubService.GetModsAsync();
        var localPaths = _localService.GetModFiles(_settings.Settings.ModsFolder);
        List<Mod>? localMods;
        try
        {
            localMods = localPaths.Select(_localService.LoadMod).Where(mod => mod is not null).ToList()!;
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_BrokenMods.Localize());
            await _localService.OpenModsFolder();
            Environment.Exit(0);
            return;
        }

        var isTracked = new bool[localMods.Count];
        foreach (var webMod in webMods)
        {
            var localMod = localMods.FirstOrDefault(x => x.Name == webMod.Name);
            var localModIdx = localMods.IndexOf(localMod!);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                webMod.IsIncompatible = !CheckCompatible(webMod);
                sourceCache.AddOrUpdate(webMod);
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

            var versionDate = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1 :
                new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
            localMod.State = (UpdateState)versionDate;
            localMod.IsShaMismatched = versionDate == 0 && webMod.SHA256 != localMod.SHA256;
            if (localMod.IsShaMismatched)
                localMod.State = UpdateState.Modified;
            localMod.IsIncompatible = !CheckCompatible(localMod);
            sourceCache.AddOrUpdate(localMod);
        }

        for (var i = 0; i < isTracked.Length; i++)
        {
            if (isTracked[i]) continue;
            var localMod = localMods[i];
            if (localMods.FirstOrDefault(x => x.Name == localMod.Name)!.IsTracked) continue;
            if (localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames =
                    string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            sourceCache.AddOrUpdate(localMods[i]);
        }
    }

    public async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_NoDownloadLink.Localize());
            return;
        }

        var errors = new StringBuilder();

        try
        {
            var path = Path.Join(_settings.Settings.ModsFolder, item.IsLocal ? item.FileNameExtended() : item.DownloadLink.Split("/")[1]);
            await _gitHubService.DownloadModAsync(item.DownloadLink, path);
            var downloadedMod = _localService.LoadMod(path)!;
            var webMods = await _gitHubService.GetModsAsync();
            var mod = webMods.FirstOrDefault(x => x.Name == downloadedMod.Name)!;
            mod.FileName = downloadedMod.FileName;
            mod.LocalVersion = downloadedMod.LocalVersion;
            _sourceCache!.AddOrUpdate(mod);
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

        var dependencies = SearchDependencies(_sourceCache!, item.Name!).ToArray();
        foreach (var dependency in dependencies)
        {
            var installedMod = Mods!.FirstOrDefault(x => x.Name == dependency.Name && x.IsLocal);
            if (installedMod is not null) continue;
            try
            {
                var path = Path.Join(_settings.Settings.ModsFolder, dependency.DownloadLink!.Split("/")[1]);
                await _gitHubService.DownloadModAsync(dependency.DownloadLink, path);
                var mod = _localService.LoadMod(path);
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
            var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
            _settings.Settings.AskEnableDependenciesWhenInstalling = await ChangeDependenciesState(
                string.Format(MsgBox_Content_EnableDependency, item.Name, disabledDependencyNames),
                disabledDependencies, _settings.Settings.AskEnableDependenciesWhenInstalling, false);
        }

        if (errors.Length > 0)
        {
            await _dialogueService.CreateErrorMessageBox(errors.ToString());
            return;
        }

        await _dialogueService.CreateMessageBox(MsgBox_Title_Success,
            string.Format(MsgBox_Content_InstallModSuccess.Localize(), item.Name));
    }

    public async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            await OnInstallMod(item);
            return;
        }

        var result = await _dialogueService.CreateConfirmMessageBox(
            string.Format(MsgBox_Content_ReinstallMod.Localize(), item.Name));
        if (!result) return;
        await OnInstallMod(item);
    }

    public async Task OnToggleMod(Mod item)
    {
        try
        {
            switch (item.IsDisabled)
            {
                case true:
                    var enabledReverseDependencies = SearchReverseDependencies(_sourceCache!, item.Name!)
                        .Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
                    if (enabledReverseDependencies.Length > 0)
                    {
                        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                        var result = await _dialogueService.CreateConfirmMessageBox(
                            string.Format(MsgBox_Content_EnableReverseDependency.Localize(), item.Name, enabledReverseDependencyNames));
                        if (!result)
                        {
                            item.IsDisabled = !item.IsDisabled;
                            return;
                        }

                        _settings.Settings.AskDisableDependenciesWhenDisabling = await ChangeDependenciesState(
                            string.Format(MsgBox_Content_DisableDependency, item.Name), enabledReverseDependencies,
                            _settings.Settings.AskDisableDependenciesWhenDisabling, true);
                    }

                    break;
                case false:
                    var disabledDependencies = SearchDependencies(_sourceCache!, item.Name!)
                        .Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
                    if (disabledDependencies.Length > 0)
                    {
                        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
                        _settings.Settings.AskEnableDependenciesWhenEnabling = await ChangeDependenciesState(
                            string.Format(MsgBox_Content_EnableDependency, item.Name, disabledDependencyNames),
                            disabledDependencies, _settings.Settings.AskEnableDependenciesWhenEnabling, false);
                    }

                    break;
            }

            File.Move(
                Path.Join(_settings.Settings.ModsFolder, item.FileNameExtended(true)),
                Path.Join(_settings.Settings.ModsFolder, item.FileNameExtended()));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed_Unauthorized.Localize(),
                        ex));
                    break;

                case IOException:
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed_Game.Localize(), ex));
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_ChangeModStateFailed.Localize(), ex));
                    break;
            }

            item.IsDisabled = !item.IsDisabled;
        }
    }

    public async Task OnDeleteMod(Mod item)
    {
        if (item.IsDuplicated)
        {
            await _dialogueService.CreateMessageBox(MsgBox_Title_Notice,
                string.Format(MsgBox_Content_DuplicateMods.Localize(), item.DuplicatedModNames), icon: Icon.Info);
            await _localService.OpenModsFolder();
            return;
        }

        var path = Path.Join(_settings.Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            await _dialogueService.CreateErrorMessageBox(MsgBox_Content_UninstallModFailed_Null);
            return;
        }

        try
        {
            var enabledReverseDependencies = SearchReverseDependencies(_sourceCache!, item.Name!)
                .Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
            if (enabledReverseDependencies.Length > 0)
            {
                var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                var result = await _dialogueService.CreateConfirmMessageBox(
                    string.Format(MsgBox_Content_UninstallDependency, item.Name, enabledReverseDependencyNames));
                if (!result)
                    return;
                _settings.Settings.AskDisableDependenciesWhenDeleting = await ChangeDependenciesState(
                    string.Format(MsgBox_Content_DisableDependency, item.Name), enabledReverseDependencies,
                    _settings.Settings.AskDisableDependenciesWhenDeleting, true);
            }

            File.Delete(path);
            _sourceCache!.Remove(item);
            var webMods = await _gitHubService.GetModsAsync();
            var webMod = webMods.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache!.AddOrUpdate(webMod);
            }

            await _dialogueService.CreateMessageBox(MsgBox_Title_Success,
                string.Format(MsgBox_Content_UninstallModSuccess.Localize(), item.Name));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_UninstallModFailed_Game.Localize(), ex));
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox(string.Format(MsgBox_Content_UninstallModFailed.Localize(), ex));
                    break;
            }
        }
    }

    private async Task CheckModToolsInstall(Mod mod)
    {
        if (_settings.Settings.AskInstallMuseDashModTools != AskType.Always) return;
        if (mod.Name != "MuseDashModTools") return;
        var result = await _dialogueService.CreateCustomConfirmMessageBox(MsgBox_Content_InstallModTools.Localize(), 3);
        if (result == MsgBox_Button_Yes) await OnInstallMod(mod);
        else if (result == MsgBox_Button_NoNoAsk) _settings.Settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
    }

    private static IEnumerable<Mod> SearchDependencies(IObservableCache<Mod, string> sourceCache, string modName)
    {
        var dependencyNames = sourceCache.Lookup(modName).Value.DependencyNames;
        return dependencyNames.Split("\r\n")
            .Where(x => sourceCache.Lookup(x).HasValue)
            .Select(x => sourceCache.Lookup(x).Value);
    }

    private static IEnumerable<Mod> SearchReverseDependencies(IObservableCache<Mod, string> sourceCache, string modName)
    {
        return sourceCache.Items.Where(x => x.DependencyNames.Split("\r\n").Contains(modName));
    }

    private async Task<AskType> ChangeDependenciesState(string content, IEnumerable<Mod> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await _dialogueService.CreateCustomConfirmMessageBox(content, 4);
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
                dependency.IsDisabled = turnOff;
                await OnToggleMod(dependency);
            }
        }

        return askType;
    }

    private bool CheckCompatible(Mod mod) =>
        mod.CompatibleGameVersion == XAML_Mod_CompatibleGameVersion || mod.GameVersion!.Contains(_currentGameVersion);
}