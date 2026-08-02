using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;
using static Euterpe.Models.Charts.ChartFiles;

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
        Logger.LogInformation("Downloading chart {Cid} ...", cid);

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, cid);
        var destinationFolder = Path.Combine(GameConfig.OnlineChartsFolder, cid);

        try
        {
            await PopulateChartWorkFolderAsync(cid, workFolder, progress, cancellationToken).ConfigureAwait(false);

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder, true))
            {
                throw new IOException($"Failed to move downloaded chart {cid} into place");
            }

            Logger.LogInformation("Chart {Cid} downloaded", cid);
            return destinationFolder;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    public async Task<string> UpdateChartAsync(string cid, IReadOnlyCollection<string> changedFiles, IReadOnlyCollection<string> deletedFiles,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating chart {Cid} ({ChangedFilesCount} changed, {DeletedFilesCount} deleted file(s)) ...", cid, changedFiles.Count, deletedFiles.Count);

        var workFolder = Path.Combine(GameConfig.TempChartsFolder, cid);
        var destinationFolder = Path.Combine(GameConfig.OnlineChartsFolder, cid);

        try
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
            FileSystemService.CopyDirectory(destinationFolder, workFolder);

            // The server omits manifest.epk when other files change; re-add it, since every change bumps it.
            var toDownload = changedFiles.Count > 0
                ? changedFiles.Append(ManifestFileName).Distinct(StringComparer.OrdinalIgnoreCase)
                : changedFiles;
            foreach (var fileName in toDownload)
            {
                await DownloadChartFileAsync(cid, workFolder, fileName, cancellationToken).ConfigureAwait(false);
            }

            foreach (var fileName in deletedFiles)
            {
                FileSystemService.TryDeleteFile(Path.Combine(workFolder, fileName));
            }

            if (!FileSystemService.TryMoveDirectory(workFolder, destinationFolder, true))
            {
                throw new IOException($"Failed to move updated chart {cid} into place");
            }

            Logger.LogInformation("Chart {Cid} updated", cid);
            return destinationFolder;
        }
        finally
        {
            FileSystemService.TryDeleteDirectory(workFolder, DeleteOption.IgnoreIfNotFound);
        }
    }

    public async Task<CheckChartUpdatesResponse> CheckChartUpdatesAsync(CheckChartUpdatesRequest request, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Checking updates for {ChartCount} chart(s) ...", request.Charts.Count);

        try
        {
            return await ChartClient.CheckChartUpdatesAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check chart updates");
            return new CheckChartUpdatesResponse();
        }
    }

    public async Task<Mod[]> FetchModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching mods ...");

        try
        {
            return await ModClient.GetModManifestAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch mod list after retries");
            return [];
        }
    }

    public async Task<Lib[]> FetchLibListAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching libs ...");

        try
        {
            return await DistributionClient.GetLatestLibsAsync(true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to fetch lib list after retries");
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
