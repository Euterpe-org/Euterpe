using System.Runtime.ExceptionServices;

namespace Euterpe.Core.Extensions;

internal static class DownloadServiceExtensions
{
    extension(DownloadService downloadService)
    {
        public async Task DownloadFileOrThrowAsync(string url, string filePath, CancellationToken cancellationToken)
        {
            Exception? failure = null;
            downloadService.DownloadFileCompleted += (_, e) => failure = e.Error;

            await downloadService.DownloadFileTaskAsync(url, filePath, cancellationToken).ConfigureAwait(false);

            var status = downloadService.Package.Status;
            if (status is DownloadStatus.Completed)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (failure is not null and not OperationCanceledException)
            {
                ExceptionDispatchInfo.Throw(failure);
            }

            throw new IOException($"Download of {url} finished with status {status}", failure);
        }
    }
}
