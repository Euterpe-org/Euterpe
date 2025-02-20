using DynamicData;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
    private async Task LoadLibsAsync()
    {
        _libsDict = LocalService.GetLibFilePaths().Select(LocalService.LoadLibFromPath).ToDictionary(x => x.Name, x => x);

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

                await DownloadManager.DownloadLibAsync(webLib.Name).ConfigureAwait(false);
                _libsDict[webLib.Name] = LocalService.LoadLibFromPath(Path.Combine(Config.UserLibsFolder, webLib.Name));
            }
            else
            {
                _libsDict[webLib.Name] = webLib.ToDto();
            }
        }
    }

    private async Task CheckLibDependenciesAsync(ModDto mod)
    {
        foreach (var lib in mod.LibDependencies)
        {
            if (_libsDict.ContainsKey(lib))
            {
                continue;
            }

            await DownloadManager.DownloadLibAsync(lib).ConfigureAwait(false);
            _libsDict[lib] = LocalService.LoadLibFromPath(Path.Combine(Config.UserLibsFolder, lib));
        }
    }

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

        await CheckLibDependenciesAsync(mod).ConfigureAwait(false);

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

        Logger.ZLogInformation($"Change mod {mod.Name} state to enabled");
        mod.IsDisabled = false;
    }

    private async Task DisableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        var modDependents = FindModDependents(mod);
        foreach (var dependent in modDependents)
        {
            if (dependent is { IsDisabled: false, IsLocal: true })
            {
                await DisableModAsync(dependent).ConfigureAwait(false);
            }
        }

        Logger.ZLogInformation($"Change mod {mod.Name} state to disabled");
        mod.IsDisabled = true;
    }

    #endregion Toggle Mod

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

    #endregion Load Mods
}