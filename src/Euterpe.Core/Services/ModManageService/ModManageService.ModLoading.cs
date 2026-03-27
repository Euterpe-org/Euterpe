namespace Euterpe.Core;

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

        foreach (var webMod in await DownloadManager.FetchModListAsync().ConfigureAwait(false))
        {
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
                var webModDto = webMod.ToDto();
                webModDto.State = IsModIncompatible(webMod.MelonVersion, webMod.GameVersion) ? ModState.Incompatible : ModState.Normal;
                _sourceCache.AddOrUpdate(webModDto);
            }
        }

        Logger.ZLogInformation($"All mods loaded");
    }

    private bool IsModIncompatible(string melonVersion, string gameVersion)
    {
        if (Config.MelonLoaderSemVersion is { } semVersion
            && !string.IsNullOrEmpty(melonVersion)
            && SemVersionRange.TryParse($"^{melonVersion}", out var range)
            && !range.Contains(semVersion))
        {
            return true;
        }

        return gameVersion is not "*" && gameVersion != Config.GameVersion;
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
            _ when IsModIncompatible(webMod.MelonVersion, webMod.GameVersion) => ModState.Incompatible,
            < 0 => ModState.Outdated,
            > 0 => ModState.Newer,
            _ when localMod.SHA256 != webMod.SHA256 => ModState.Modified,
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
            .Where(group => group.Select(mod => mod.LocalFileName).Distinct().Skip(1).Any());

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

    private void RefreshModStates()
    {
        foreach (var modDto in _sourceCache.Items)
        {
            if (!modDto.HasDownloadSource || modDto.State is not (ModState.Normal or ModState.Incompatible))
            {
                continue;
            }

            modDto.State = IsModIncompatible(modDto.MelonVersion, modDto.GameVersion) ? ModState.Incompatible : ModState.Normal;
        }

        _sourceCache.Refresh();
        Logger.ZLogInformation($"Mod states refreshed with MelonLoader version: {Config.MelonLoaderSemVersion}");
    }
}