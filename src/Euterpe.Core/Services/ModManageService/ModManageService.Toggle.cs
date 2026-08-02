namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task<bool> EnableModAsync(ModDto mod)
    {
        if (!FileSystemService.TryMoveFile(
                Path.Combine(GameConfig.ModsFolder, mod.LocalFileName),
                Path.Combine(GameConfig.ModsFolder, mod.ReversedFileName)))
        {
            Logger.LogError("Failed to enable mod {ModName}: could not move file {ModLocalFileName}", mod.Name, mod.LocalFileName);
            return false;
        }

        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);

        mod.IsDisabled = false;
        Logger.LogInformation("Change mod {ModName} state to enabled", mod.Name);

        return true;
    }

    private async Task EnableModDependenciesAsync(ModDto mod)
    {
        foreach (var dependency in FindModDependencies(mod))
        {
            if (!dependency.IsLocal)
            {
                await InstallModAsync(dependency).ConfigureAwait(false);
            }
            else if (dependency.IsDisabled)
            {
                await EnableModAsync(dependency).ConfigureAwait(false);
            }
        }
    }

    private async Task<bool> DisableModAsync(ModDto mod)
    {
        if (!FileSystemService.TryMoveFile(
                Path.Combine(GameConfig.ModsFolder, mod.LocalFileName),
                Path.Combine(GameConfig.ModsFolder, mod.ReversedFileName)))
        {
            Logger.LogError("Failed to disable mod {ModName}: could not move file {ModLocalFileName}", mod.Name, mod.LocalFileName);
            return false;
        }

        await DisableModDependentsAsync(mod).ConfigureAwait(false);

        mod.IsDisabled = true;
        Logger.LogInformation("Change mod {ModName} state to disabled", mod.Name);

        return true;
    }

    private async Task DisableModDependentsAsync(ModDto mod)
    {
        foreach (var dependent in FindModDependents(mod).Where(static dependent => dependent is { IsDisabled: false, IsLocal: true }))
        {
            await DisableModAsync(dependent).ConfigureAwait(false);
        }
    }
}
