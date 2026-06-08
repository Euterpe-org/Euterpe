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
            await DownloadVerifiedAsync(url, workPath, expectedSha256, displayName, cancellationToken).ConfigureAwait(false);

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

    private async Task DownloadVerifiedAsync(string url, string workPath, string? expectedSha256, string displayName, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await AppDownloadManager.DownloadAssetAsync(url, workPath, displayName, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.ZLogWarning(ex, $"Attempt {attempt}/{MaxRetries}: download of {displayName} failed");
                continue;
            }

            if (string.IsNullOrEmpty(expectedSha256))
            {
                return;
            }

            var actualSha256 = await SHA256Utils.HexLowerFromPathAsync(workPath).ConfigureAwait(false);
            if (string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Logger.ZLogWarning($"Attempt {attempt}/{MaxRetries}: checksum mismatch for {displayName}, expected {expectedSha256}, got {actualSha256}");
        }

        throw new IOException($"Failed to download a valid {displayName} after {MaxRetries} attempts");
    }
}