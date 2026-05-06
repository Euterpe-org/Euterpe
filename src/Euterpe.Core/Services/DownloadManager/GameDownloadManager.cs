using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Core;

internal sealed class GameDownloadManager : IGameDownloadManager
{
    public Task DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(GameConfig.ModsFolder, mod.FileName);

        return AppDownloadManager.DownloadAssetAsync(mod.DownloadUrl, path, $"mod {mod.Name}", cancellationToken);
    }

    public Task DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(GameConfig.UserLibsFolder, lib.FileName);

        return AppDownloadManager.DownloadAssetAsync(lib.DownloadUrl, path, $"lib {lib.Name}", cancellationToken);
    }

    public async Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching mods ...");

        try
        {
            return await ModClient.GetModManifestAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch mod list after retries");
            return [];
        }
    }

    public async Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Fetching libs ...");

        try
        {
            return await DistributionClient.GetLatestLibsAsync(true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch lib list after retries");
            return [];
        }
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IAppDownloadManager AppDownloadManager { get; init; }

    [UsedImplicitly]
    public required IEuterpeDistributionClient DistributionClient { get; init; }

    [UsedImplicitly]
    public required ILogger<GameDownloadManager> Logger { get; init; }

    [UsedImplicitly]
    public required IEuterpeModClient ModClient { get; init; }

    #endregion Injections
}