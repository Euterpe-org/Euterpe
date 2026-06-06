namespace Euterpe.Abstractions;

public interface IChartManageService
{
    IObservable<IChangeSet<ChartDto, string>> Connect();
    Task InitializeChartsAsync();
}