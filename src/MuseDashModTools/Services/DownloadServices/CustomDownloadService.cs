namespace MuseDashModTools.Services;

public sealed class CustomDownloadService : ICustomDownloadService
{
    #region Injections

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections

    public Task CheckForUpdatesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}