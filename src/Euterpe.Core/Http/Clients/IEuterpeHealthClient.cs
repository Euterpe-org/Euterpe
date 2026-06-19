using Refit;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeHealthClient
{
    [Get(EuterpeWeb.Health)]
    Task<HttpResponseMessage> CheckAsync(CancellationToken cancellationToken = default);
}
