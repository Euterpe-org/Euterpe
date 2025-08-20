namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
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
}