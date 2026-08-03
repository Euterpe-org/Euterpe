using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeLogClient
{
    [Multipart]
    [Post(Logs.Upload)]
    Task<HttpResponseMessage> UploadLogAsync(StreamPart file, string category, CancellationToken cancellationToken = default);
}
