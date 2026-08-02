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
            Logger.LogInformation("{DependencyName} already exists and hash matches, skipping download", spec.Name);
            return;
        }

        await DownloadUtils.DownloadVerifiedAsync(
            ct => AppDownloadManager.DownloadFileAsync(spec.Url, spec.FilePath, onDownloadStarted, progress, ct),
            spec.FilePath, spec.ExpectedHash, spec.Name, MaxRetries, Logger, cancellationToken).ConfigureAwait(false);
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
