using Euterpe.Models.Dependencies;

namespace Euterpe.Core;

internal sealed partial class DependencyAcquireService
{
    private async Task EnsureValidAsync(
        DependencySpec spec,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (await IsValidAsync(spec.FilePath, spec.ExpectedHash).ConfigureAwait(false))
        {
            Logger.ZLogInformation($"{spec.Name} already exists and hash matches, skipping download");
            return;
        }

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var success = await DownloadManager.DownloadDependencyAsync(spec, onDownloadStarted, progress, cancellationToken).ConfigureAwait(false);
            if (!success)
            {
                Logger.ZLogWarning($"Attempt {attempt}/{MaxRetries}: Download of {spec.Name} failed");
                continue;
            }

            if (await IsValidAsync(spec.FilePath, spec.ExpectedHash).ConfigureAwait(false))
            {
                Logger.ZLogInformation($"{spec.Name} download completed successfully");
                return;
            }

            var actualHash = await SHA512Utils.HexFromPathAsync(spec.FilePath).ConfigureAwait(false);
            Logger.ZLogWarning($"Attempt {attempt}/{MaxRetries}: {spec.Name} hash mismatch after download\r\nExpected: {spec.ExpectedHash}\r\nActual: {actualHash}");
        }

        throw new InvalidOperationException($"Failed to download a valid {spec.Name} after {MaxRetries} attempts.");
    }

    private static async Task<bool> IsValidAsync(string filePath, string expectedHash)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var actualHash = await SHA512Utils.HexFromPathAsync(filePath).ConfigureAwait(false);
        return actualHash == expectedHash;
    }
}