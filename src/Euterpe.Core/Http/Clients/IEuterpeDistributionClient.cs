using Refit;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeDistributionClient
{
    [Get(EuterpeApi.Distribution.Libs.Manifest)]
    Task<Lib[]> GetLibManifestAsync(CancellationToken cancellationToken = default);
}