using System.Text;

namespace Euterpe.Core;

internal sealed partial class ImageCacheService
{
    private async Task<(byte[] Bytes, bool Cached)?> DownloadAsync(Uri source, string filePath)
    {
        try
        {
            var bytes = await Client.GetByteArrayAsync(source).ConfigureAwait(false);
            var cached = await FileSystemService.TryWriteFileAtomicAsync(filePath, bytes).ConfigureAwait(false);
            return (bytes, cached);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"Failed to download image {source}");
            return null;
        }
    }

    private static string CacheFileName(Uri source) =>
        SHA256Utils.HexLowerFromBytes(Encoding.UTF8.GetBytes(source.AbsoluteUri));
}
