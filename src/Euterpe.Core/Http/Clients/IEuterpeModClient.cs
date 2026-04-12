using Refit;
using Mod = Euterpe.Models.Mod;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeModClient
{
    [Get(EuterpeApi.Mod.Manifest)]
    Task<Mod[]> GetModManifestAsync(CancellationToken cancellationToken = default);
}