using System.Collections.Concurrent;
using AsyncAwaitBestPractices;
using DynamicData;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
    #region Load Mods

    private async Task LoadModsAsync()
    {
        ModDto[] localMods = LocalService.GetModFilePaths()
            .Select(LocalService.LoadModFromPath)
            .Where(mod => mod is not null)
            .ToArray()!;

        _sourceCache.AddOrUpdate(localMods);
        Logger.ZLogInformation($"Local mods added to source cache");

        CheckDuplicatedMods(localMods);

        await foreach (var webMod in DownloadManager.GetModListAsync())
        {
            if (webMod is null)
            {
                continue;
            }

            if (_sourceCache.Lookup(webMod.Name) is { HasValue: true, Value: var localMod })
            {
                CheckModState(localMod, webMod);
                localMod.UpdateFromMod(webMod);
                CheckConfigFile(localMod);

                if (!localMod.IsDisabled)
                {
                    CheckLibDependencies(localMod);
                }

                _sourceCache.AddOrUpdate(localMod);
            }
            else
            {
                _sourceCache.AddOrUpdate(webMod.ToDto());
            }
        }

        Logger.ZLogInformation($"All mods loaded");
    }

    private void CheckModState(ModDto localMod, Mod webMod)
    {
        if (localMod.State == ModState.Duplicated)
        {
            return;
        }

        var localVersion = SemVersion.Parse(localMod.LocalVersion);
        var webVersion = SemVersion.Parse(webMod.Version);
        var versionComparison = localVersion.ComparePrecedenceTo(webVersion);

        localMod.State = versionComparison switch
        {
            < 0 => ModState.Outdated,
            > 0 => ModState.Newer,
            _ when localMod.SHA256 != webMod.SHA256 => ModState.Modified,
            _ when webMod.GameVersion != "*" && webMod.GameVersion != _gameVersion => ModState.Incompatible,
            _ => ModState.Normal
        };
    }

    private void CheckConfigFile(ModDto localMod)
    {
        if (localMod.ConfigFile.IsNullOrEmpty())
        {
            return;
        }

        var configFilePath = Path.Combine(Config.UserDataFolder, localMod.ConfigFile);
        localMod.IsValidConfigFile = File.Exists(configFilePath);
    }

    private void CheckDuplicatedMods(ModDto[] localMods)
    {
        var duplicatedModGroups = localMods
            .GroupBy(mod => mod.Name)
            .Where(group => group.Select(mod => mod.LocalFileName).Distinct().Count() > 1);

        foreach (var duplicatedModGroup in duplicatedModGroups)
        {
            var modName = duplicatedModGroup.Key;
            Logger.ZLogInformation($"Duplicated mod found {modName}");

            var localMod = _sourceCache.Lookup(modName).Value;
            localMod.State = ModState.Duplicated;
            localMod.DuplicatedModPaths = duplicatedModGroup.Select(mod => mod.LocalFileName).ToArray();
        }

        Logger.ZLogInformation($"Checking duplicated mods finished");
    }

    #endregion Load Mods #region Toggle Mod

    #region Load Libs

    private async Task LoadLibsAsync()
    {
        _libsDict = new ConcurrentDictionary<string, LibDto>(
            LocalService.GetLibFilePaths()
                .Select(LocalService.LoadLibFromPath)
                .Select(x => new KeyValuePair<string, LibDto>(x.Name, x)));

        await foreach (var webLib in DownloadManager.GetLibListAsync())
        {
            if (webLib is null)
            {
                continue;
            }

            if (_libsDict.TryGetValue(webLib.Name, out var localLib))
            {
                if (localLib.SHA256 == webLib.SHA256)
                {
                    continue;
                }

                // TODO MessageBox (lxy, 2025/2/21)
                DownloadLibAsync(webLib.ToDto()).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Download lib {webLib.Name} failed"));
            }
            else
            {
                _libsDict[webLib.Name] = webLib.ToDto();
            }
        }

        Logger.ZLogInformation($"All libs loaded");
    }

    private void CheckLibDependencies(ModDto mod)
    {
        foreach (var libName in mod.LibDependencies)
        {
            var lib = _libsDict[libName];
            if (lib.IsLocal)
            {
                continue;
            }

            DownloadLibAsync(lib).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Download lib {libName} failed"));
        }
    }

    private async Task DownloadLibAsync(LibDto lib)
    {
        await DownloadManager.DownloadLibAsync(lib).ConfigureAwait(false);
        _libsDict[lib.Name] = LocalService.LoadLibFromPath(Path.Combine(Config.UserLibsFolder, lib.FileName));
        Logger.ZLogInformation($"Lib {lib.Name} download finished");
    }

    #endregion Load Libs

    #region Dependencies and Dependents

    /// <summary>
    ///     Find mods that the given mod depends on
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private IEnumerable<ModDto> FindModDependencies(ModDto mod)
    {
        Logger.ZLogInformation($"Finding mod dependencies for {mod.Name}");
        return mod.ModDependencies.Select(x => _sourceCache.Lookup(x).Value);
    }

    /// <summary>
    ///     Find mods that depend on the given mod
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private IEnumerable<ModDto> FindModDependents(ModDto mod)
    {
        Logger.ZLogInformation($"Finding mod dependents for {mod.Name}");
        return _sourceCache.Items.Where(x => x.ModDependencies.Contains(mod.Name));
    }

    #endregion Dependencies and Dependents

    #region Toggle Mod

    private async Task EnableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);

        Logger.ZLogInformation($"Change mod {mod.Name} state to enabled");
        mod.IsDisabled = false;
    }

    private async Task EnableModDependenciesAsync(ModDto mod)
    {
        var modDependencies = FindModDependencies(mod);
        foreach (var dependency in modDependencies)
        {
            if (dependency is { IsDisabled: true, IsLocal: true })
            {
                await EnableModAsync(dependency).ConfigureAwait(false);
            }
            else if (!dependency.IsLocal)
            {
                await InstallModAsync(dependency).ConfigureAwait(false);
            }
        }
    }

    private async Task DisableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        await DisableModDependentsAsync(mod).ConfigureAwait(false);

        Logger.ZLogInformation($"Change mod {mod.Name} state to disabled");
        mod.IsDisabled = true;
    }

    private async Task DisableModDependentsAsync(ModDto mod)
    {
        var modDependents = FindModDependents(mod);
        foreach (var dependent in modDependents)
        {
            if (dependent is { IsDisabled: false, IsLocal: true })
            {
                await DisableModAsync(dependent).ConfigureAwait(false);
            }
        }
    }

    #endregion Toggle Mod
}