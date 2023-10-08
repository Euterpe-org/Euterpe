using System.Collections.ObjectModel;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Tabs;

public partial class ChartDownloadViewModel : ViewModelBase, IChartDownloadViewModel
{
    private readonly ReadOnlyObservableCollection<Chart> _charts;
    private readonly SourceCache<Chart, string> _sourceCache = new(x => x.Name);
    [ObservableProperty] private string _filter;
    private ChartSortOptions _sortOption;

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
            .Filter(x => string.IsNullOrEmpty(Filter)
                         || x.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                         || x.Author.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                         || x.Charter.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .SortBy(GetSortByOption)
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

    private void SortBy(ChartSortOptions option)
    {
        _sortOption = option;
        _sourceCache.Refresh();
    }

    private IComparable GetSortByOption(Chart chart)
    {
        return _sortOption switch
        {
            ChartSortOptions.Id => chart.Id,
            ChartSortOptions.Name => chart.Name,
            ChartSortOptions.Downloads => -chart.Analytics.Downloads,
            ChartSortOptions.Likes => -chart.Analytics.LikesCount,
            ChartSortOptions.Level => -chart.GetHighestLevel(),
            ChartSortOptions.Latest => -chart.Id,
            _ => ChartSortOptions.Id
        };
    }

    #region Commands

    [RelayCommand]
    private async Task DownloadChart(Chart item) => await ChartService.DownloadChart(item);

    [RelayCommand]
    private async Task OpenCustomAlbumsFolder() => await LocalService.OpenCustomAlbumsFolder();

    [RelayCommand]
    private void SortById() => SortBy(ChartSortOptions.Id);

    [RelayCommand]
    private void SortByName() => SortBy(ChartSortOptions.Name);

    [RelayCommand]
    private void SortByDownloads() => SortBy(ChartSortOptions.Downloads);

    [RelayCommand]
    private void SortByLikes() => SortBy(ChartSortOptions.Likes);

    [RelayCommand]
    private void SortByLevel() => SortBy(ChartSortOptions.Level);

    [RelayCommand]
    private void SortByLatest() => SortBy(ChartSortOptions.Latest);

    #endregion
}