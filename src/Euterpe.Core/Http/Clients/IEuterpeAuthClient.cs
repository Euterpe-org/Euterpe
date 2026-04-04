using Euterpe.Contracts.Account;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

internal interface IEuterpeAuthClient
{
    [Post(Auth.AppToken)]
    Task<AppTokenResponse> ExchangeAppTokenAsync([Body] AppTokenRequest request, CancellationToken cancellationToken = default);

    [Post(Auth.Refresh)]
    Task<RefreshResponse> RefreshTokenAsync([Body] RefreshRequest request, CancellationToken cancellationToken = default);

    [Post(Auth.Logout)]
    Task LogoutAsync([Body] LogoutRequest request, CancellationToken cancellationToken = default);

    [Get(Auth.Me)]
    Task<UserInfo> GetMeAsync([Authorize] string token, CancellationToken cancellationToken = default);
}