namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
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
            localMod.DuplicatedModPaths = string.Join(Environment.NewLine, duplicatedModGroup.Select(mod => mod.LocalFileName));
        }

        Logger.ZLogInformation($"Checking duplicated mods finished");
    }

    private async Task CheckLibDependencies(ModDto mod)
    {
        foreach (var lib in mod.DependentLibs)
        {
            if (_libNames.Contains(lib))
            {
                continue;
            }

            await DownloadManager.DownloadLibAsync(lib);
            _libNames.Add(lib);
        }
    }

    private async Task EnableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        await CheckLibDependencies(mod);

        var modDependencies = FindModDependencies(mod);
        foreach (var dependency in modDependencies)
        {
            if (dependency is { IsDisabled: true, IsLocal: true })
            {
                await EnableModAsync(dependency);
            }
            else if (!dependency.IsLocal)
            {
                await InstallModAsync(dependency);
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
                await DisableModAsync(dependent);
            }
        }

        Logger.ZLogInformation($"Change mod {mod.Name} state to disabled");
        mod.IsDisabled = true;
    }

    /// <summary>
    ///     Find mods that the given mod depends on
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private IEnumerable<ModDto> FindModDependencies(ModDto mod)
    {
        Logger.ZLogInformation($"Finding mod dependencies for {mod.Name}");
        return mod.DependentMods.Select(x => _sourceCache.Lookup(x).Value);
    }

    /// <summary>
    ///     Find mods that depend on the given mod
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    private IEnumerable<ModDto> FindModDependents(ModDto mod)
    {
        Logger.ZLogInformation($"Finding mod dependents for {mod.Name}");
        return _sourceCache.Items.Where(x => x.DependentMods.Contains(mod.Name));
    }
}