namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private ModDto[] GetInstalledMods() =>
        _sourceCache.Items.Where(static mod => mod.IsLocal).ToArray();

    private ModDto[] GetEnabledMods() =>
        _sourceCache.Items.Where(static mod => mod is { IsLocal: true, IsDisabled: false }).ToArray();

    private ModDto[] GetOutdatedMods() =>
        _sourceCache.Items.Where(static mod => mod.State is ModState.Outdated).ToArray();

    private ModDto[] FindModDependencies(ModDto mod) =>
        mod.ModDependencies.Select(x => _sourceCache.Lookup(x).Value).ToArray();

    private ModDto[] FindModDependents(ModDto mod) =>
        _sourceCache.Items.Where(x => x.ModDependencies.Contains(mod.Name)).ToArray();
}
