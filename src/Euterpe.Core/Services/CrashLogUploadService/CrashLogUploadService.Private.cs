using Refit;

namespace Euterpe.Core;

internal sealed partial class CrashLogUploadService
{
    private Task<HttpResponseMessage> UploadAsync(Stream compressedLogStream)
    {
        compressedLogStream.Position = 0;
        var file = new StreamPart(compressedLogStream, $"{Path.GetFileName(LogFilePath)}.gz", GzipContentType);
        return LogClient.UploadLogAsync(file, AppCategory);
    }

    private void LogResult(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            Logger.LogInformation("Crash log uploaded successfully");
            return;
        }

        Logger.LogWarning("Crash log upload returned HTTP {StatusCode}", (int)response.StatusCode);
    }
}
