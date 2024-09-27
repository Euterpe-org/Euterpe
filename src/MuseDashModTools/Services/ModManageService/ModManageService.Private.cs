namespace MuseDashModTools.Services;

public sealed partial class ModManageService
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

    private void CheckDuplicatedMods(ModDto[] localMods)
    {
        var duplicatedModGroups = localMods
            .GroupBy(mod => mod.Name)
            .Where(group => group.Select(mod => mod.FileName).Distinct().Count() > 1)
            .ToArray();

        foreach (var duplicatedModGroup in duplicatedModGroups)
        {
            var localMod = _sourceCache.Lookup(duplicatedModGroup.Key).Value;
            localMod.State = ModState.Duplicated;
            localMod.DuplicatedModPaths = string.Join(Environment.NewLine, duplicatedModGroup.Select(mod => mod.FileName));
        }

        Logger.Information("Duplicated mods initialized");
    }
}