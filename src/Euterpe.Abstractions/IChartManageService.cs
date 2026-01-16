namespace Euterpe.Abstractions;

public interface IChartManageService
{
    Task InitializeChartsAsync(SourceCache<Chart, string> sourceCache);
}