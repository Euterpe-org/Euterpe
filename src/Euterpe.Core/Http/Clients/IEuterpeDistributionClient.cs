using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeDistributionClient
{
    [Get($"{Distribution.Libs.Path}{Distribution.Libs.Manifest}")]
    Task<Lib[]> GetLibManifestAsync(CancellationToken cancellationToken = default);
}