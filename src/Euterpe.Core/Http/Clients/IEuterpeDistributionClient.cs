using Euterpe.Contracts.Distribution;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Core.Http.Clients;

public interface IEuterpeDistributionClient
{
    [Get(Distribution.LibsPath)]
    Task<Lib[]> GetLatestLibsAsync([Query] bool latest = true, CancellationToken cancellationToken = default);

    [Get(Distribution.DependenciesPath)]
    Task<Dependency[]> GetLatestDependenciesAsync([Query] bool latest = true, CancellationToken cancellationToken = default);

    [Get(Distribution.ReleasesPath)]
    Task<Release[]> GetAppReleaseAsync([Query] bool latest = true, [Query] bool prerelease = false, CancellationToken cancellationToken = default);
}
