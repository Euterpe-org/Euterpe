using System.Collections.Concurrent;
using AsyncAwaitBestPractices;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task LoadLibsAsync()
    {
        _libsDict = new ConcurrentDictionary<string, LibDto>(
            (await GameLocalService.GetLibFilePaths()
                .WhenAllAsync(GameLocalService.LoadLibFromPathAsync).ConfigureAwait(false))
            .Select(x => KeyValuePair.Create(x.Name, x)));

        foreach (var webLib in await GameDownloadManager.FetchLibListAsync().ConfigureAwait(false))
        {
            if (_libsDict.TryGetValue(webLib.Slug, out var localLib))
            {
                var webLibDto = webLib.ToModel();
                if (localLib.SHA256 == webLibDto.SHA256)
                {
                    continue;
                }

                DownloadLibAsync(webLibDto).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Download lib {webLib.Slug} failed"));
            }
            else
            {
                _libsDict[webLib.Slug] = webLib.ToModel();
            }
        }

        Logger.ZLogInformation($"All libs loaded");
    }

    private void CheckLibDependencies(ModDto mod)
    {
        foreach (var libName in mod.LibDependencies)
        {
            var lib = _libsDict[libName];
            if (lib.IsLocal)
            {
                continue;
            }

            DownloadLibAsync(lib).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Download lib {libName} failed"));
        }
    }

    private Task DownloadLibAsync(LibDto lib) =>
        _singleFlight.RunAsync(lib.Name, () => DownloadLibCoreAsync(lib));

    private async Task DownloadLibCoreAsync(LibDto lib)
    {
        await GameDownloadManager.DownloadLibAsync(lib).ConfigureAwait(false);
        _libsDict[lib.Name] = await GameLocalService.LoadLibFromPathAsync(Path.Combine(GameConfig.UserLibsFolder, lib.FileName)).ConfigureAwait(false);
        Logger.ZLogInformation($"Lib {lib.Name} download finished");
    }
}