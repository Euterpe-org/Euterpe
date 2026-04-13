using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeModClient
{
    [Get(Mods.Manifest)]
    Task<Mod[]> GetModManifestAsync(CancellationToken cancellationToken = default);
}