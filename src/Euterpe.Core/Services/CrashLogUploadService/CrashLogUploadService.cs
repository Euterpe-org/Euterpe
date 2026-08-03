using System.IO.Compression;

namespace Euterpe.Core;

internal sealed partial class CrashLogUploadService : ICrashLogUploadService
{
    private const string AppCategory = "app";
    private const string GzipContentType = "application/gzip";

    public async Task UploadAppLogAsync()
    {
        try
        {
            var logStream = FileSystemService.TryOpenSharedReadFile(LogFilePath);
            if (logStream is null)
            {
                return;
            }

            await using (logStream.ConfigureAwait(false))
            {
                var compressedLogStream = FileSystemService.TryCreateTemporaryFile();
                if (compressedLogStream is null)
                {
                    return;
                }

                await using (compressedLogStream.ConfigureAwait(false))
                {
                    var gzipStream = new GZipStream(compressedLogStream, CompressionLevel.Optimal, true);
                    await using (gzipStream.ConfigureAwait(false))
                    {
                        await logStream.CopyToAsync(gzipStream).ConfigureAwait(false);
                    }

                    using var response = await UploadAsync(compressedLogStream).ConfigureAwait(false);
                    LogResult(response);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to upload crash log");
        }
    }

    #region Injections

    public required IEuterpeLogClient LogClient { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<CrashLogUploadService> Logger { get; init; }

    #endregion Injections
}
