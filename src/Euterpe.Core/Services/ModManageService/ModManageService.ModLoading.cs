using Euterpe.Contracts.Mods;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task LoadWebModsAsync()
    {
        foreach (var webMod in await GameDownloadManager.FetchModListAsync().ConfigureAwait(false))
        {
            CacheWebMod(webMod);
        }

        Logger.ZLogInformation($"All mods loaded");
    }

    private void CacheWebMod(Mod webMod)
    {
        if (_sourceCache.Lookup(webMod.Name) is not { HasValue: true, Value: var localMod })
        {
            _sourceCache.AddOrUpdate(webMod.ToModel());
            return;
        }

        localMod.UpdateFromMod(webMod);
        CheckModFiles(localMod);
    }

    private void CheckModFiles(ModDto localMod)
    {
        CheckConfigFile(localMod);
        if (!localMod.IsDisabled)
        {
            CheckLibDependencies(localMod);
        }
    }

    private void CheckConfigFile(ModDto localMod) =>
        localMod.IsValidConfigFile = !localMod.ConfigFile.IsNullOrEmpty()
                                     && File.Exists(Path.Combine(GameConfig.UserDataFolder, localMod.ConfigFile));
}
