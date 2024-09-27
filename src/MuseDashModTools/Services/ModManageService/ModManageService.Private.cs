namespace MuseDashModTools.Services;

public sealed partial class ModManageService
{
    private void CheckModState(ModDto localMod, Mod webMod)
    {
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
}