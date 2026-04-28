namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task<bool> EnableModAsync(ModDto mod)
    {
        if (!FileSystemService.TryMoveFile(
                Path.Combine(GameConfig.ModsFolder, mod.LocalFileName),
                Path.Combine(GameConfig.ModsFolder, mod.ReversedFileName)))
        {
            Logger.ZLogError($"Failed to enable mod {mod.Name}: could not move file {mod.LocalFileName}");
            return false;
        }

        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);

        mod.IsDisabled = false;
        Logger.ZLogInformation($"Change mod {mod.Name} state to enabled");

        return true;
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

    private async Task<bool> DisableModAsync(ModDto mod)
    {
        if (!FileSystemService.TryMoveFile(
                Path.Combine(GameConfig.ModsFolder, mod.LocalFileName),
                Path.Combine(GameConfig.ModsFolder, mod.ReversedFileName)))
        {
            Logger.ZLogError($"Failed to disable mod {mod.Name}: could not move file {mod.LocalFileName}");
            return false;
        }

        await DisableModDependentsAsync(mod).ConfigureAwait(false);

        mod.IsDisabled = true;
        Logger.ZLogInformation($"Change mod {mod.Name} state to disabled");

        return true;
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