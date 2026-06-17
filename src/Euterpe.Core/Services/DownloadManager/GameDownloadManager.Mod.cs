namespace Euterpe.Core;

internal sealed partial class GameDownloadManager
{
    private async Task DownloadAssetAtomicAsync(
        string url,
        string destinationFolder,
        string fileName,
        string? expectedSha256,
        string displayName,
        CancellationToken cancellationToken)
    {
        var workPath = Path.Combine(GameConfig.TempModsFolder, fileName);
        var destinationPath = Path.Combine(destinationFolder, fileName);

        try
        {
            await DownloadUtils.DownloadVerifiedAsync(
                ct => AppDownloadManager.DownloadAssetAsync(url, workPath, displayName, ct),
                workPath, expectedSha256, displayName, MaxRetries, Logger, cancellationToken).ConfigureAwait(false);

            Directory.CreateDirectory(destinationFolder);
            if (!FileSystemService.TryMoveFile(workPath, destinationPath, true))
            {
                throw new IOException($"Failed to move downloaded {displayName} into place");
            }
        }
        finally
        {
            FileSystemService.TryDeleteFile(workPath, DeleteOption.IgnoreIfNotFound);
        }
    }
}
