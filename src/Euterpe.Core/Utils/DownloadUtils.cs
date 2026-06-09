namespace Euterpe.Core.Utils;

public static class DownloadUtils
{
    /// <summary>
    ///     Runs <paramref name="download" /> up to <paramref name="maxRetries" /> times, verifying the SHA-256 of the
    ///     resulting file at <paramref name="filePath" /> after each successful attempt. When
    ///     <paramref name="expectedSha256" /> is null or empty the first successful download is accepted without
    ///     verification. Cancellation propagates immediately; any other download failure is retried.
    /// </summary>
    /// <exception cref="IOException">No valid file was produced after <paramref name="maxRetries" /> attempts.</exception>
    public static async Task DownloadVerifiedAsync(
        Func<CancellationToken, Task> download,
        string filePath,
        string? expectedSha256,
        string displayName,
        int maxRetries,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await download(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.ZLogWarning(ex, $"Attempt {attempt}/{maxRetries}: download of {displayName} failed");
                continue;
            }

            if (string.IsNullOrEmpty(expectedSha256))
            {
                return;
            }

            var actualSha256 = await SHA256Utils.HexLowerFromPathAsync(filePath).ConfigureAwait(false);
            if (string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            logger.ZLogWarning($"Attempt {attempt}/{maxRetries}: checksum mismatch for {displayName}, expected {expectedSha256}, got {actualSha256}");
        }

        throw new IOException($"Failed to download a valid {displayName} after {maxRetries} attempts");
    }
}
