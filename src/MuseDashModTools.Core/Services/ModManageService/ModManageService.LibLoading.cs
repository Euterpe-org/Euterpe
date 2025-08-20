using System.Collections.Concurrent;
using AsyncAwaitBestPractices;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService
{
    private async Task LoadLibsAsync()
    {
        _libsDict = new ConcurrentDictionary<string, LibDto>(
            (await LocalService.GetLibFilePaths()
                .WhenAllAsync(LocalService.LoadLibFromPathAsync!).ConfigureAwait(false))
            .Select(x => new KeyValuePair<string, LibDto>(x!.Name, x)));

        await foreach (var webLib in DownloadManager.GetLibListAsync().ConfigureAwait(false))
        {
            if (webLib is null)
            {
                continue;
            }

            if (_libsDict.TryGetValue(webLib.Name, out var localLib))
            {
                if (localLib.SHA256 == webLib.SHA256)
                {
                    continue;
                }

                DownloadLibAsync(webLib.ToDto()).SafeFireAndForget(ex => Logger.ZLogError(ex, $"Download lib {webLib.Name} failed"));
            }
            else
            {
                _libsDict[webLib.Name] = webLib.ToDto();
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

    private async Task DownloadLibAsync(LibDto lib)
    {
        await DownloadManager.DownloadLibAsync(lib).ConfigureAwait(false);
        _libsDict[lib.Name] = await LocalService.LoadLibFromPathAsync(Path.Combine(Config.UserLibsFolder, lib.FileName)).ConfigureAwait(false);
        Logger.ZLogInformation($"Lib {lib.Name} download finished");
    }
}