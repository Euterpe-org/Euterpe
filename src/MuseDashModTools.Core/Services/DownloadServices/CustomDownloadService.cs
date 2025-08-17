namespace MuseDashModTools.Core;

internal sealed class CustomDownloadService : ICustomDownloadService
{
    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task DownloadReleaseByTagAsync(string tag, string osString, string updateFolder, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    #region Injections

    [UsedImplicitly]
    public required ILogger<CustomDownloadService> Logger { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}