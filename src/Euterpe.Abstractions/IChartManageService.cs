using Euterpe.Contracts.Charts;

namespace Euterpe.Abstractions;

public interface IChartManageService
{
    Task InitializeChartsAsync(SourceCache<Chart, string> sourceCache);
}