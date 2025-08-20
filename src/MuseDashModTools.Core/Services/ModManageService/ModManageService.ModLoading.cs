namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
    private async Task LoadModsAsync()
    {
        ModDto[] localMods = (await LocalService.GetModFilePaths()
                .WhenAllAsync(LocalService.LoadModFromPathAsync).ConfigureAwait(false))
            .Where(x => x is not null)
            .ToArray()!;

        _sourceCache.AddOrUpdate(localMods);
        Logger.ZLogInformation($"Local mods added to source cache");

        CheckDuplicatedMods(localMods);

        await foreach (var webMod in DownloadManager.GetModListAsync().ConfigureAwait(false))
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
        if (localMod.State is ModState.Duplicated)
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
            _ when webMod.GameVersion is not "*" && webMod.GameVersion != _gameVersion => ModState.Incompatible,
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
}