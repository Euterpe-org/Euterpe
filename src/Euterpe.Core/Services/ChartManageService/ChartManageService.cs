namespace Euterpe.Core;

internal sealed partial class ChartManageService : IChartManageService
{
    private readonly Lazy<Task> _initTask;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderPath);

    public ChartManageService() => _initTask = new Lazy<Task>(LoadChartsAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ChartDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeChartsAsync() => _initTask.Value;

    #region Injections

    public required IChartLocalService ChartLocalService { get; init; }
    public required ILogger<ChartManageService> Logger { get; init; }

    #endregion Injections
}