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
        var mod = ModManageService.Value.FindModByName(modName);
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

        await ModManageService.Value.InstallModAsync(mod).ConfigureAwait(false);
    }

    private async Task UninstallModByNameAsync(string modName)
    {
        var mod = ModManageService.Value.FindModByName(modName);
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

        await ModManageService.Value.UninstallModAsync(mod).ConfigureAwait(false);
    }
}