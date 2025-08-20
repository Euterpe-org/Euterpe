namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
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
}