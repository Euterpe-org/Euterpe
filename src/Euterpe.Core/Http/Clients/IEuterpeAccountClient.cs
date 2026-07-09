using Euterpe.Contracts.Account;
using Refit;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeAccountClient
{
    [Get("")]
    Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
