namespace Euterpe.Abstractions;

public interface IRemoteImageLoader
{
    /// <summary>
    ///     Opens a remote image, downloading and caching it when missing. The caller owns the returned stream.
    /// </summary>
    Task<Stream?> OpenReadAsync(Uri source, CancellationToken cancellationToken = default);
}
