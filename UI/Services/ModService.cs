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
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class ModService : IModService
{
    private readonly IDialogueService _dialogueService;
    private readonly IGitHubService _gitHubService;
    private readonly ISettingService _settings;
    private readonly ILocalService _localService;

    private SourceCache<Mod, string>? _sourceCache;
    private ReadOnlyObservableCollection<Mod>? Mods;
    private string? _currentGameVersion;

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
            await _dialogueService.CreateErrorMessageBox("Your downloaded mods are broken\nPlease delete 0kb mod if it exist");
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
                localMod.DuplicatedModNames = string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
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

            var versionDate = new Version(webMod.Version!) > new Version(localMod.LocalVersion!) ? -1 : new Version(webMod.Version!) < new Version(localMod.LocalVersion!) ? 1 : 0;
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
                localMod.DuplicatedModNames = string.Join("\r\n", localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            sourceCache.AddOrUpdate(localMods[i]);
        }

        bool CheckCompatible(Mod mod)
        {
            return mod.CompatibleGameVersion == "All" || mod.GameVersion!.Contains(_currentGameVersion);
        }
    }

    public async Task CheckModToolsInstall(Mod mod)
    {
        if (_settings.Settings.AskInstallMuseDashModTools != AskType.Always) return;
        if (mod.Name != "MuseDashModTools") return;
        var result = await _dialogueService.CreateCustomConfirmMessageBox("You don't have MuseDashModTools mod installed\nWhich checks available update for all the mods when launching Muse Dash\nInstall Now?");
        switch (result)
        {
            case "Yes":
                await OnInstallMod(mod);
                break;
            case "No and Don't Ask Again":
                _settings.Settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
                break;
        }
    }

    public async Task OnInstallMod(Mod item)
    {
        if (item.DownloadLink is null)
        {
            await _dialogueService.CreateErrorMessageBox("This mod does not have an available resource for download.\n");
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
                    errors.AppendLine($"Mod installation failed\nAre you online? {ex}");
                    break;

                case SecurityException:
                case UnauthorizedAccessException:
                case IOException:
                    errors.AppendLine($"Mod installation failed\nIs the game running? {ex}");
                    break;

                default:
                    errors.AppendLine($"Mod installation failed\n{ex}");
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
                errors.AppendLine($"Dependency failed to install\n {ex}");
            }
        }

        var disabledDependencies = dependencies.Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
        if (disabledDependencies.Length > 0)
        {
            var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
            _settings.Settings.AskEnableDependenciesWhenInstalling = await ChangeDependenciesState(_sourceCache!,
                $"Do you want to enable {item.Name}'s dependency {disabledDependencyNames}?",
                disabledDependencies, _settings.Settings.AskEnableDependenciesWhenInstalling, false);
        }

        if (errors.Length > 0)
        {
            await _dialogueService.CreateErrorMessageBox(errors.ToString());
            return;
        }

        await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully installed\n");
    }

    public async Task OnReinstallMod(Mod item)
    {
        if (item.State == UpdateState.Outdated)
        {
            await OnInstallMod(item);
            return;
        }

        var result = await _dialogueService.CreateConfirmMessageBox($"You are asking to reinstall {item.Name}\nPlease confirm your operation");
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
                    var enabledReverseDependencies = SearchReverseDependencies(_sourceCache!, item.Name!).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
                    if (enabledReverseDependencies.Length > 0)
                    {
                        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                        var result = await _dialogueService.CreateConfirmMessageBox($"{item.Name} is used by {enabledReverseDependencyNames} as dependency\nAre you sure you want to disable this mod?");
                        if (!result)
                        {
                            item.IsDisabled = !item.IsDisabled;
                            return;
                        }

                        _settings.Settings.AskDisableDependenciesWhenDisabling = await ChangeDependenciesState(_sourceCache!,
                            $"Do you want to disable the mods depend on {item.Name} as well?",
                            enabledReverseDependencies, _settings.Settings.AskDisableDependenciesWhenDisabling, true);
                    }

                    break;
                case false:
                    var disabledDependencies = SearchDependencies(_sourceCache!, item.Name!).Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
                    if (disabledDependencies.Length > 0)
                    {
                        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));
                        _settings.Settings.AskEnableDependenciesWhenEnabling = await ChangeDependenciesState(_sourceCache!,
                            $"Do you want to enable {item.Name}'s dependency {disabledDependencyNames} as well?",
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
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nUnauthorized");
                    break;

                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod disable/enable failed\n");
                    break;
            }

            item.IsDisabled = !item.IsDisabled;
        }
    }

    public async Task OnDeleteMod(Mod item)
    {
        if (item.IsDuplicated)
        {
            await _dialogueService.CreateMessageBox("Notice", $"Please manually choose and delete the duplicated mod\n{item.DuplicatedModNames}", icon: Icon.Info);
            await _localService.OpenModsFolder();
            return;
        }

        var path = Path.Join(_settings.Settings.ModsFolder, item.FileNameExtended());
        if (!File.Exists(path))
        {
            await _dialogueService.CreateErrorMessageBox("Cannot delete file that doesn't exist");
            return;
        }

        try
        {
            var enabledReverseDependencies = SearchReverseDependencies(_sourceCache!, item.Name!).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
            if (enabledReverseDependencies.Length > 0)
            {
                var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x.Name));
                var result = await _dialogueService.CreateConfirmMessageBox($"{item.Name} is used by {enabledReverseDependencyNames} as dependency\nAre you sure you want to delete this mod?");
                if (!result)
                    return;
                _settings.Settings.AskDisableDependenciesWhenDeleting = await ChangeDependenciesState(_sourceCache!,
                    $"Do you want to disable the mods depend on {item.Name}?",
                    enabledReverseDependencies, _settings.Settings.AskDisableDependenciesWhenDeleting, true);
            }

            File.Delete(path);
            _sourceCache!.Remove(item);
            var webMods = await _gitHubService.GetModsAsync();
            var webMod = webMods.FirstOrDefault(x => x.Name == item.Name);
            if (webMod is not null)
            {
                _sourceCache!.AddOrUpdate(webMod);
            }

            await _dialogueService.CreateMessageBox("Success", $"{item.Name} has been successfully deleted.\n");
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                case IOException:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed\nIs the game running?");
                    break;

                default:
                    await _dialogueService.CreateErrorMessageBox("Mod uninstall failed");
                    break;
            }
        }
    }

    private IEnumerable<Mod> SearchDependencies(IObservableCache<Mod, string> sourceCache, string modName)
    {
        var dependencyNames = sourceCache.Lookup(modName).Value.DependencyNames;
        var dependencies = dependencyNames.Split("\r\n")
            .Where(x => sourceCache.Lookup(x).HasValue)
            .Select(x => sourceCache.Lookup(x).Value);
        return dependencies;
    }

    private IEnumerable<Mod> SearchReverseDependencies(IObservableCache<Mod, string> sourceCache, string modName)
    {
        var reverseDependencies = sourceCache.Items.Where(x => x.DependencyNames.Split("\r\n").Contains(modName));
        return reverseDependencies;
    }

    private async Task<AskType> ChangeDependenciesState(SourceCache<Mod, string> sourceCache, string content, IEnumerable<Mod> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await _dialogueService.CreateCustomConfirmMessageBox(content, 4);
                switch (askResult)
                {
                    case "Yes":
                        await ChangeState();
                        break;
                    case "Yes and Don't ask Again":
                        await ChangeState();
                        askType = AskType.YesAndNoAsk;
                        break;
                    case "No and Don't ask Again":
                        askType = AskType.NoAndNoAsk;
                        break;
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
}