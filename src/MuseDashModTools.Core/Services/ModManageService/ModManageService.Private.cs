namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
    private void CheckModState(ModDto localMod, Mod webMod)
    {
        if (localMod.IsDuplicated)
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
            _ when webMod.GameVersion is not ["*"] && !webMod.GameVersion.Contains(_gameVersion) => ModState.Incompatible,
            _ => ModState.Normal
        };
    }

    private void CheckConfigFile(ModDto localMod)
    {
        if (localMod.ConfigFile.IsNullOrEmpty())
        {
            return;
        }

        var configFilePath = Path.Combine(Setting.UserDataFolder, localMod.ConfigFile);
        if (File.Exists(configFilePath))
        {
            localMod.IsValidConfigFile = true;
        }
    }

    private void CheckDuplicatedMods(ModDto[] localMods)
    {
        var duplicatedModGroups = localMods
            .GroupBy(mod => mod.Name)
            .Where(group => group.Select(mod => mod.FileName).Distinct().Count() > 1);

        foreach (var duplicatedModGroup in duplicatedModGroups)
        {
            var modName = duplicatedModGroup.Key;
            Logger.ZLogInformation($"Duplicated mod found {modName}");

            var localMod = _sourceCache.Lookup(modName).Value;
            localMod.State = ModState.Duplicated;
            localMod.DuplicatedModPaths = string.Join(Environment.NewLine, duplicatedModGroup.Select(mod => mod.FileName));
        }

        Logger.ZLogInformation($"Checking duplicated mods finished");
    }
}