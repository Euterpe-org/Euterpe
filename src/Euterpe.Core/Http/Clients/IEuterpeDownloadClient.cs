using Refit;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeDownloadClient
{
    [Get("")]
    Task DownloadFileAsync([Query] string token, CancellationToken cancellationToken = default);
}