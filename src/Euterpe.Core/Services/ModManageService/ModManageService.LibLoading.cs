using System.Collections.Concurrent;
using AsyncAwaitBestPractices;
using Euterpe.Contracts.Distribution;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task LoadLibsAsync()
    {
        _libsDict = new ConcurrentDictionary<string, LibDto>(
            (await ModLocalService.GetLibFilePaths()
                .WhenAllAsync(ModLocalService.LoadLibFromPathAsync).ConfigureAwait(false))
            .Select(static lib => KeyValuePair.Create(lib.Name, lib)));

        foreach (var webLib in await GameDownloadManager.FetchLibListAsync().ConfigureAwait(false))
        {
            CacheWebLib(webLib);
        }

        Logger.LogInformation("All libs loaded");
    }

    private void CacheWebLib(Lib webLib)
    {
        if (!_libsDict.TryGetValue(webLib.Slug, out var localLib))
        {
            _libsDict[webLib.Slug] = webLib.ToModel();
            return;
        }

        var webLibDto = webLib.ToModel();
        if (localLib.SHA256 != webLibDto.SHA256)
        {
            DownloadLibAsync(webLibDto).SafeFireAndForget(ex => Logger.LogError(ex, "Download lib {LibSlug} failed", webLib.Slug));
        }
    }

    private void CheckLibDependencies(ModDto mod)
    {
        foreach (var lib in mod.LibDependencies.Select(libName => _libsDict[libName]).Where(static lib => !lib.IsLocal))
        {
            DownloadLibAsync(lib).SafeFireAndForget(ex => Logger.LogError(ex, "Download lib {LibName} failed", lib.Name));
        }
    }

    private Task DownloadLibAsync(LibDto lib) =>
        _singleFlight.RunAsync(lib.Name, () => DownloadLibCoreAsync(lib));

    private async Task DownloadLibCoreAsync(LibDto lib)
    {
        await GameDownloadManager.DownloadLibAsync(lib).ConfigureAwait(false);
        _libsDict[lib.Name] = await ModLocalService.LoadLibFromPathAsync(Path.Combine(GameConfig.UserLibsFolder, lib.FileName)).ConfigureAwait(false);
        Logger.LogInformation("Lib {LibName} download finished", lib.Name);
    }
}
