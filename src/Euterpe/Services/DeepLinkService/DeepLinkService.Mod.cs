namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private async Task HandleModActionAsync(string path)
    {
        var segments = path.Split('/', 2);

        switch (segments)
        {
            case ["install", var modName]:
                await InstallModByNameAsync(modName).ConfigureAwait(false);
                break;

            case ["update"]:
                await ModManageService.UpdateAllModsAsync().ConfigureAwait(false);
                break;

            case ["update", var modName]:
                await UpdateModByNameAsync(modName).ConfigureAwait(false);
                break;

            case ["uninstall", var modName]:
                await UninstallModByNameAsync(modName).ConfigureAwait(false);
                break;

            default:
                Logger.ZLogWarning($"Unknown mod deep link path: {path}");
                break;
        }
    }

    private async Task InstallModByNameAsync(string modName)
    {
        var mod = ModManageService.FindModByName(modName);
        if (mod is null)
        {
            Logger.ZLogWarning($"Deep link: mod '{modName}' not found");
            return;
        }

        if (mod.IsLocal)
        {
            Logger.ZLogInformation($"Deep link: mod '{modName}' is already installed");
            return;
        }

        await ModManageService.InstallModAsync(mod).ConfigureAwait(false);
    }

    private async Task UpdateModByNameAsync(string modName)
    {
        var mod = ModManageService.FindModByName(modName);
        if (mod is null)
        {
            Logger.ZLogWarning($"Deep link: mod '{modName}' not found");
            return;
        }

        if (!mod.IsLocal)
        {
            Logger.ZLogInformation($"Deep link: mod '{modName}' is not installed");
            return;
        }

        if (mod.State is not ModState.Outdated)
        {
            Logger.ZLogInformation($"Deep link: mod '{modName}' is not outdated and cannot be updated");
            return;
        }

        await ModManageService.UpdateModAsync(mod).ConfigureAwait(false);
    }

    private async Task UninstallModByNameAsync(string modName)
    {
        var mod = ModManageService.FindModByName(modName);
        if (mod is null)
        {
            Logger.ZLogWarning($"Deep link: mod '{modName}' not found");
            return;
        }

        if (!mod.IsLocal)
        {
            Logger.ZLogInformation($"Deep link: mod '{modName}' is not installed");
            return;
        }

        await ModManageService.UninstallModAsync(mod).ConfigureAwait(false);
    }
}