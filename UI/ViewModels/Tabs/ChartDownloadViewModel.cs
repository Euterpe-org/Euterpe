#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ChartDownloadViewModel : ViewModelBase, IChartDownloadViewModel
{
    [ObservableProperty] private List<Chart>? _charts;

    [UsedImplicitly]
    public IGitHubService GitHubService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public async Task Initialize()
    {
        Charts = await GitHubService.GetChartList();
        Logger.Information("Chart Download Window Initialized");
    }
}