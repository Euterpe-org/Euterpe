using Euterpe.Contracts.Account;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeAccountClient
{
    [Get("")]
    Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    [Put(Account.VanillaBinding)]
    Task BindVanillaAccountAsync([Body] MuseDashUidRequest request, CancellationToken cancellationToken = default);
}