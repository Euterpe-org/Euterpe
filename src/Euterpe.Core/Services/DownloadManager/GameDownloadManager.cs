using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Core;

internal sealed partial class GameDownloadManager : IGameDownloadManager
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

    public async Task<string> DownloadChartAsync(string cid, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading chart {cid} ...");

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, cid);
        var destinationFolder = Path.Combine(GameConfig.OnlineChartsFolder, cid);

        try
        {
            await PopulateChartWorkFolderAsync(cid, workFolder, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder, true))
            {
                throw new IOException($"Failed to move downloaded chart {cid} into place");
            }

            Logger.ZLogInformation($"Chart {cid} downloaded");
            return destinationFolder;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
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

    public required GameConfig GameConfig { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IEuterpeDistributionClient DistributionClient { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required ILogger<GameDownloadManager> Logger { get; init; }
    public required IEuterpeModClient ModClient { get; init; }

    #endregion Injections
}