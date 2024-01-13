using System.IO;
using System.Net.Http;
using System.Security;
using System.Text;
using DynamicData;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Services;

public sealed partial class ModService
{
    /// <summary>
    ///     Handle exception when install mod
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="errors"></param>
    private static void HandleInstallModException(Exception ex, StringBuilder errors)
    {
        switch (ex)
        {
            case HttpRequestException:
                errors.AppendFormat(MsgBox_Content_InstallModFailed_Internet, ex).AppendLine();
                break;

            case SecurityException:
            case UnauthorizedAccessException:
            case IOException:
                errors.AppendFormat(MsgBox_Content_InstallModFailed_Game, ex).AppendLine();
                break;

            default:
                errors.AppendFormat(MsgBox_Content_InstallModFailed, ex).AppendLine();
                break;
        }
    }

    /// <summary>
    ///     Handle exception when toggle mod
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ex"></param>
    private async Task HandleToggleModException(Mod item, Exception ex)
    {
        var errorMsg = ex switch
        {
            UnauthorizedAccessException => MsgBox_Content_ChangeModStateFailed_Unauthorized,
            IOException => MsgBox_Content_ChangeModStateFailed_Game,
            _ => MsgBox_Content_ChangeModStateFailed
        };

        Logger.Error(ex, "Change mod {Name} state failed", item.Name);
        await MessageBoxService.ErrorMessageBox(errorMsg, ex);

        item.IsDisabled = !item.IsDisabled;
    }

    /// <summary>
    ///     Handle exception when delete mod
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ex"></param>
    private async Task HandleDeleteModException(Mod item, Exception ex)
    {
        var errorMsg = string.Format(
            ex is UnauthorizedAccessException or IOException
                ? MsgBox_Content_UninstallModFailed_Game
                : MsgBox_Content_UninstallModFailed, ex);

        Logger.Error(ex, "Delete mod {Name} failed", item.Name);
        await MessageBoxService.ErrorMessageBox(errorMsg);
    }

    #region InitializeModList Private Methods

    /// <summary>
    ///     Load local mods and show on the UI
    /// </summary>
    private async Task LoadModsToUI()
    {
        _isTracked = new bool[_localMods.Count];
        foreach (var webMod in _webMods!)
        {
            var localMod = _localMods.Find(x => x.Name == webMod.Name);
            var localModIdx = _localMods.IndexOf(localMod!);

            if (localMod is null)
            {
                webMod.IsTracked = true;
                webMod.IsIncompatible = !CheckCompatible(webMod);
                _sourceCache?.AddOrUpdate(webMod);
                await CheckModToolsInstall(webMod);
                continue;
            }

            if (_localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames = string.Join(
                    "\r\n", _localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
            }

            _isTracked[localModIdx] = true;
            localMod.IsTracked = true;
            localMod.Version = webMod.Version;
            localMod.CloneOnlineInfo(webMod);

            CheckConfigFileExist(localMod);
            CheckVersionState(webMod, localMod);
            _sourceCache?.AddOrUpdate(localMod);
            Logger?.Information("Mod {Name} loaded to UI", localMod.Name);
        }

        CheckDuplicatedMods();
    }

    /// <summary>
    ///     Check if the mod's config file path is valid
    /// </summary>
    /// <param name="localMod"></param>
    private void CheckConfigFileExist(Mod localMod)
    {
        if (string.IsNullOrEmpty(localMod.ConfigFile))
        {
            return;
        }

        var configFile = Path.Join(SavingService.Settings.UserDataFolder, localMod.ConfigFile);
        localMod.IsValidConfigFile = File.Exists(configFile);
    }

    /// <summary>
    ///     Check the mod's version state
    /// </summary>
    /// <param name="webMod"></param>
    /// <param name="localMod"></param>
    private void CheckVersionState(Mod webMod, Mod localMod)
    {
        var versionState = SemanticVersion.Parse(webMod.Version) > SemanticVersion.Parse(localMod.LocalVersion!) ? -1
            : SemanticVersion.Parse(webMod.Version) < SemanticVersion.Parse(localMod.LocalVersion!) ? 1 : 0;
        localMod.State = (UpdateState)versionState;
        localMod.IsShaMismatched = versionState == 0 && webMod.SHA256 != localMod.SHA256;
        if (localMod.IsShaMismatched)
        {
            localMod.State = UpdateState.Modified;
        }

        localMod.IsIncompatible = !CheckCompatible(localMod);
    }

    /// <summary>
    ///     Check duplicated mods
    /// </summary>
    private void CheckDuplicatedMods()
    {
        for (var i = 0; i < _isTracked.Length; i++)
        {
            if (_isTracked[i])
            {
                continue;
            }

            var localMod = _localMods[i];
            if (_localMods.Find(x => x.Name == localMod.Name)!.IsTracked)
            {
                continue;
            }

            if (_localMods.Count(x => x.Name == localMod.Name) > 1)
            {
                localMod.IsDuplicated = true;
                localMod.DuplicatedModNames = string.Join("\r\n",
                    _localMods.Where(x => x.Name == localMod.Name).Select(x => x.FileNameExtended()));
                Logger.Information("Found duplicated mod {DuplicateMods}", localMod.DuplicatedModNames);
            }

            _sourceCache?.AddOrUpdate(_localMods[i]);
        }
    }

    /// <summary>
    ///     Check Mod Tools install
    /// </summary>
    /// <param name="mod"></param>
    private async Task CheckModToolsInstall(Mod mod)
    {
        if (SavingService.Settings.AskInstallMuseDashModTools != AskType.Always)
        {
            return;
        }

        if (mod.Name != "MuseDashModTools")
        {
            return;
        }

        var result = await MessageBoxService.CustomConfirmMessageBox(MsgBox_Content_InstallModTools, 3);
        if (result == MsgBox_Button_Yes)
        {
            await OnInstallModAsync(mod);
        }
        else if (result == MsgBox_Button_NoNoAsk)
        {
            SavingService.Settings.AskInstallMuseDashModTools = AskType.NoAndNoAsk;
        }
    }

    /// <summary>
    ///     Check if the mod is compatible with the current game version
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private bool CheckCompatible(Mod mod) =>
        mod.CompatibleGameVersion == XAML_Mod_CompatibleGameVersion || mod.GameVersion!.Contains(_currentGameVersion);

    #endregion

    #region Dependency Private Methods

    /// <summary>
    ///     Check if the mod's dependencies are installed
    /// </summary>
    /// <param name="item"></param>
    /// <returns>StringBuilder for errors</returns>
    private async Task<StringBuilder> CheckDependencyInstall(Mod item)
    {
        var dependencies = SearchDependencies(item.Name).ToArray();
        var errors = new StringBuilder();
        foreach (var dependency in dependencies)
        {
            var installedMod = _mods.FirstOrDefault(x => x.Name == dependency.Name && x.IsLocal);
            if (installedMod is not null)
            {
                continue;
            }

            try
            {
                var path = Path.Join(SavingService.Settings.ModsFolder, dependency.DownloadLink.Split("/")[1]);
                await GitHubService.DownloadModAsync(dependency.DownloadLink, path);
                var downloadedMod = LocalService.LoadMod(path);
                dependency.IsDisabled = downloadedMod!.IsDisabled;
                dependency.FileName = downloadedMod.FileName;
                dependency.LocalVersion = downloadedMod.LocalVersion;
                dependency.SHA256 = downloadedMod.SHA256;
                CheckConfigFileExist(dependency);
                Logger.Information("Install dependency {Name} success", downloadedMod.Name);
                _sourceCache.AddOrUpdate(dependency);
                await CheckDependencyInstall(dependency);
            }
            catch (Exception ex)
            {
                errors.AppendFormat(MsgBox_Content_InstallDependencyFailed, dependency.Name, ex).AppendLine();
                Logger.Information(ex, "Install dependency {Name} failed", dependency.Name);
            }
        }

        SettingsViewModel.EnableDependenciesWhenInstalling = (int)await EnableDependencies(item, dependencies,
            MsgBox_Content_EnableDependency, SavingService.Settings.AskEnableDepWhenInstall);
        return errors;
    }

    /// <summary>
    ///     Enable Dependencies
    /// </summary>
    /// <param name="item"></param>
    /// <param name="dependencies"></param>
    /// <param name="message"></param>
    /// <param name="askType"></param>
    /// <returns></returns>
    private async Task<AskType> EnableDependencies(Mod item, IEnumerable<Mod> dependencies, string message, AskType askType)
    {
        var disabledDependencies = dependencies.Where(x => x is { IsLocal: true, IsDisabled: true }).ToArray();
        if (disabledDependencies.Length == 0)
        {
            return askType;
        }

        var disabledDependencyNames = string.Join(", ", disabledDependencies.Select(x => x.Name));

        return await ChangeDependenciesState(string.Format(message, item.Name, disabledDependencyNames),
            disabledDependencies, askType, false);
    }

    /// <summary>
    ///     Disable Reverse Dependencies
    /// </summary>
    /// <param name="item"></param>
    /// <param name="message"></param>
    /// <param name="askType"></param>
    /// <returns></returns>
    private async Task<(bool, AskType)> DisableReverseDependencies(Mod item, string message, AskType askType)
    {
        var enabledReverseDependencies = SearchReverseDependencies(item.Name).Where(x => x is { IsLocal: true, IsDisabled: false }).ToArray();
        if (enabledReverseDependencies.Length == 0)
        {
            return (true, askType);
        }

        var enabledReverseDependencyNames = string.Join(", ", enabledReverseDependencies.Select(x => x?.Name));

        var result = await MessageBoxService.FormatWarningConfirmMessageBox(message, item.Name, enabledReverseDependencyNames);
        if (!result)
        {
            item.IsDisabled = result;
            return (result, askType);
        }

        return (true, await ChangeDependenciesState(string.Format(MsgBox_Content_DisableReverseDependency, item.Name),
            enabledReverseDependencies, askType, true));
    }

    /// <summary>
    ///     Search dependencies
    /// </summary>
    /// <param name="modName"></param>
    /// <returns>Dependencies</returns>
    private IEnumerable<Mod> SearchDependencies(string modName)
    {
        var dependencyNames = _sourceCache.Lookup(modName).Value.DependencyNames.Split("\r\n");
        Logger.Information("Search dependencies of {ModName}: {DependencyNames}", modName, dependencyNames);
        return dependencyNames.Where(x => _sourceCache.Lookup(x).HasValue).Select(x => _sourceCache.Lookup(x).Value);
    }

    /// <summary>
    ///     Search reverse dependencies
    /// </summary>
    /// <param name="modName"></param>
    /// <returns>Reverse dependencies</returns>
    private IEnumerable<Mod?> SearchReverseDependencies(string modName)
    {
        var reverseDependencyNames = _sourceCache.Items.Where(x => x.DependencyNames.Split("\r\n").Contains(modName))
            .Select(x => x.Name).ToArray();
        Logger.Information("Search reverse dependencies of {ModName}: {ReverseDependencyNames}", modName,
            reverseDependencyNames);
        return _sourceCache.Items.Where(x => reverseDependencyNames.Contains(x.Name));
    }

    /// <summary>
    ///     Change dependency states according to ask type
    /// </summary>
    /// <param name="content"></param>
    /// <param name="dependencies"></param>
    /// <param name="askType">Saved AskType</param>
    /// <param name="turnOff"></param>
    /// <returns></returns>
    private async Task<AskType> ChangeDependenciesState(string content, IEnumerable<Mod?> dependencies, AskType askType, bool turnOff)
    {
        switch (askType)
        {
            case AskType.Always:
                var askResult = await MessageBoxService.CustomConfirmMessageBox(content, 4);
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
            if (dependency!.IsDisabled == turnOff)
            {
                continue;
            }

            dependency.IsDisabled = turnOff;
            await OnToggleModAsync(dependency);
        }
    }

    #endregion
}