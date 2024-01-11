using System.Collections.ObjectModel;
using DynamicData;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels.Pages;

public sealed partial class ChartDownloadViewModel : ViewModelBase, IChartDownloadViewModel
{
    private readonly ReadOnlyObservableCollection<Chart> _charts;
    private readonly SourceCache<Chart, string> _sourceCache = new(x => x.Name);
    [ObservableProperty] private List<ChartFilterType> _categoryChartFilterTypes = new();
    [ObservableProperty] private int _currentSortOptionIndex;
    [ObservableProperty] private string _filter;
    [ObservableProperty] private string[] _sortOptions;
    public ReadOnlyObservableCollection<Chart> Charts => _charts;

    [UsedImplicitly]
    public IChartService ChartService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    public ChartDownloadViewModel()
    {
        _sourceCache.Connect().Filter(x => string.IsNullOrEmpty(Filter) ||
                                           x.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase) ||
                                           x.Author.Contains(Filter, StringComparison.OrdinalIgnoreCase) ||
                                           x.Charter.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .Filter(x => !CategoryChartFilterTypes.Contains(ChartFilterType.Easy) || x.HasEasy)
            .Filter(x => !CategoryChartFilterTypes.Contains(ChartFilterType.Hard) || x.HasHard)
            .Filter(x => !CategoryChartFilterTypes.Contains(ChartFilterType.Master) || x.HasMaster)
            .Filter(x => !CategoryChartFilterTypes.Contains(ChartFilterType.Hidden) || x.HasHidden)
            .SortBy(GetSortByOption)
            .Bind(out _charts)
            .Subscribe();
    }

    public ChartSortOptions SortOption { get; private set; }

    public async Task Initialize()
    {
        SortOptions =
        [
            XAML_ChartSortOption_Default, XAML_ChartSortOption_Name,
            XAML_ChartSortOption_Downloads, XAML_ChartSortOption_Likes,
            XAML_ChartSortOption_Level, XAML_ChartSortOption_Latest
        ];
        await ChartService.InitializeChartList(_sourceCache, Charts);
        Logger.Information("Chart Download Window Initialized");
    }

    private IComparable GetSortByOption(Chart chart)
    {
        return SortOption switch
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

    private void FilterBy(ChartFilterType filterType)
    {
        if (!CategoryChartFilterTypes.Remove(filterType))
        {
            CategoryChartFilterTypes.Add(filterType);
        }

        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnCurrentSortOptionIndexChanged(int value)
    {
        if (value == -1)
        {
            return;
        }

        SortOption = (ChartSortOptions)value;
        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnFilterChanged(string value) => _sourceCache.Refresh();

    #region Commands

    [RelayCommand]
    private async Task DownloadChartAsync(Chart item) => await ChartService.DownloadChartAsync(item);

    [RelayCommand]
    private async Task OpenCustomAlbumsFolderAsync() => await LocalService.OpenCustomAlbumsFolderAsync();

    [RelayCommand]
    private void OnFilterEasy() => FilterBy(ChartFilterType.Easy);

    [RelayCommand]
    private void OnFilterHard() => FilterBy(ChartFilterType.Hard);

    [RelayCommand]
    private void OnFilterMaster() => FilterBy(ChartFilterType.Master);

    [RelayCommand]
    private void OnFilterHidden() => FilterBy(ChartFilterType.Hidden);

    #endregion
}