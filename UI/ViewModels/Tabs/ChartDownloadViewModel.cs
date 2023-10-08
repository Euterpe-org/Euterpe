using System.Collections.ObjectModel;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ChartDownloadViewModel : ViewModelBase, IChartDownloadViewModel
{
    private readonly ReadOnlyObservableCollection<Chart> _charts;
    private readonly SourceCache<Chart, string> _sourceCache = new(x => x.Name);
    [ObservableProperty] private string _filter;
    public ReadOnlyObservableCollection<Chart> Charts => _charts;

    [UsedImplicitly]
    public IChartService ChartService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public ChartDownloadViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => string.IsNullOrEmpty(_filter)
                         || x.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                         || x.Charter.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .SortBy(x => x.Id)
            .Bind(out _charts)
            .Subscribe();
    }

    public async Task Initialize()
    {
        await ChartService.InitializeChartList(_sourceCache, Charts);
        Logger.Information("Chart Download Window Initialized");
    }

    [UsedImplicitly]
    partial void OnFilterChanged(string value) => _sourceCache.Refresh();

    [RelayCommand]
    private async Task DownloadChart(Chart item) => await ChartService.DownloadChart(item);

    [RelayCommand]
    private async Task OpenCustomAlbumsFolder() => await LocalService.OpenCustomAlbumsFolder();
}