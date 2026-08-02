using Euterpe.Contracts.Credits;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeCreditsClient
{
    [Get(Public.Credits)]
    Task<CreditsResponse> GetCreditsAsync([Query] string lang, CancellationToken cancellationToken = default);
}
