using Euterpe.Shared.Threading;

namespace Euterpe.Core;

internal sealed partial class ImageCacheService : IRemoteImageLoader
{
    private readonly SingleFlight<string> _singleFlight = new();

    public async Task<Stream?> OpenReadAsync(Uri source, CancellationToken cancellationToken = default)
    {
        if (!source.IsAbsoluteUri)
        {
            return null;
        }

        var filePath = Path.Combine(Config.CacheFolder, "Images", CacheFileName(source));
        if (FileSystemService.GetFileLastWriteTimeUtc(filePath) is not null
            && FileSystemService.TryOpenReadFile(filePath) is { } cachedStream)
        {
            return cachedStream;
        }

        var download = await _singleFlight.RunAsync(filePath, () => DownloadAsync(source, filePath)).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (download is null)
        {
            return null;
        }

        return download.Value.Cached
            ? FileSystemService.TryOpenReadFile(filePath) ?? new MemoryStream(download.Value.Bytes, false)
            : new MemoryStream(download.Value.Bytes, false);
    }

    #region Injections

    public required HttpClient Client { get; init; }
    public required Config Config { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<ImageCacheService> Logger { get; init; }

    #endregion Injections
}
