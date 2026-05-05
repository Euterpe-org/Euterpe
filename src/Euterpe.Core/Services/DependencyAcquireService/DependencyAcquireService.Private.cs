using Euterpe.Contracts.Distribution;

namespace Euterpe.Core;

internal sealed partial class DependencyAcquireService
{
    private async Task<Dependency[]> GetDependenciesAsync(CancellationToken cancellationToken) =>
        _cachedDependencies ??= await DistributionClient.GetLatestDependenciesAsync(true, cancellationToken).ConfigureAwait(false);

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
            var success = await AppDownloadManager.DownloadFileAsync(spec.Url, spec.FilePath, onDownloadStarted, progress, cancellationToken).ConfigureAwait(false);
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

            var actualHash = await SHA256Utils.HexLowerFromPathAsync(spec.FilePath).ConfigureAwait(false);
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

        var actualHash = await SHA256Utils.HexLowerFromPathAsync(filePath).ConfigureAwait(false);
        return actualHash == expectedHash;
    }

    private sealed record DependencyTarget(string Slug, string FilePath);

    private sealed record DependencySpec(string Name, string Url, string FilePath, string ExpectedHash);
}