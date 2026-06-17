using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;

namespace Euterpe.Core;

internal sealed partial class GameDownloadManager : IGameDownloadManager
{
    private const int MaxRetries = 3;

    public Task DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) =>
        DownloadAssetAtomicAsync(mod.DownloadUrl, GameConfig.ModsFolder, mod.FileName, mod.SHA256, $"mod {mod.Name}", cancellationToken);

    public Task DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default) =>
        DownloadAssetAtomicAsync(lib.DownloadUrl, GameConfig.UserLibsFolder, lib.FileName, lib.SHA256, $"lib {lib.Name}", cancellationToken);

    public async Task<string> DownloadChartAsync(string cid, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Downloading chart {cid} ...");

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, cid);
        var destinationFolder = Path.Combine(GameConfig.OnlineChartsFolder, cid);

        try
        {
            await PopulateChartWorkFolderAsync(cid, workFolder, progress, cancellationToken).ConfigureAwait(false);

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

    public async Task<string> UpdateChartAsync(string cid, IReadOnlyCollection<string> changedFiles, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Updating chart {cid} ({changedFiles.Count} changed file(s)) ...");

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, cid);
        var destinationFolder = Path.Combine(GameConfig.OnlineChartsFolder, cid);

        try
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
            FileSystemService.CopyDirectory(destinationFolder, workFolder);

            foreach (var fileName in changedFiles)
            {
                await DownloadChartFileAsync(cid, workFolder, fileName, cancellationToken).ConfigureAwait(false);
            }

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder, true))
            {
                throw new IOException($"Failed to move updated chart {cid} into place");
            }

            Logger.ZLogInformation($"Chart {cid} updated");
            return destinationFolder;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    public async Task<CheckChartUpdatesResponse> CheckChartUpdatesAsync(CheckChartUpdatesRequest request, CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Checking updates for {request.Charts.Count} chart(s) ...");

        try
        {
            return await ChartClient.CheckChartUpdatesAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check chart updates");
            return new CheckChartUpdatesResponse();
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
    public required IEuterpeChartClient ChartClient { get; init; }
    public required IEuterpeDistributionClient DistributionClient { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IMessagePackSerializationService MessagePackSerialization { get; init; }
    public required ILogger<GameDownloadManager> Logger { get; init; }
    public required IEuterpeModClient ModClient { get; init; }

    #endregion Injections
}
