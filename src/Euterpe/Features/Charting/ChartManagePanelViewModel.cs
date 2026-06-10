using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Platform.Storage;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGame]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private const int RatingLowerBound = 1;
    private const int RatingUpperBound = 12;

    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly BehaviorSubject<IComparer<ChartDto>> _comparer;
    private readonly Subject<Unit> _searchTextChanged = new();
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderName);

    private ChartDto? _playingChart;
    private ChartDto? _pausedChart;

    [ObservableProperty]
    public partial int SelectedChartSourceIndex { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty] public partial bool ShowEasy { get; set; } = true;
    [ObservableProperty] public partial bool ShowHard { get; set; } = true;
    [ObservableProperty] public partial bool ShowMaster { get; set; } = true;
    [ObservableProperty] public partial bool ShowHidden { get; set; } = true;

    [ObservableProperty] public partial int? RatingMin { get; set; } = RatingLowerBound;
    [ObservableProperty] public partial int? RatingMax { get; set; } = RatingUpperBound;

    [ObservableProperty] public partial int? BpmMin { get; set; }
    [ObservableProperty] public partial int? BpmMax { get; set; }

    [ObservableProperty] public partial bool StreamerSafeOnly { get; set; }
    [ObservableProperty] public partial bool HasVideoOnly { get; set; }

    [ObservableProperty] public partial ChartSortField SortField { get; set; }
    [ObservableProperty] public partial bool SortDescending { get; set; }

    [ObservableProperty] public partial bool AllChartsLoaded { get; set; }

    public ReadOnlyObservableCollection<ChartDto> Charts => _charts;

    public ChartManagePanelViewModel()
    {
        _comparer = new BehaviorSubject<IComparer<ChartDto>>(BuildComparer());

        _sourceCache.Connect()
            .Filter(MatchesFilters)
            .SortAndBind(out _charts, _comparer.AsSystemObservable())
            .Subscribe();

        _searchTextChanged
            .Debounce(TimeSpan.FromMilliseconds(300))
            .Subscribe(this, static (_, vm) => vm.RefreshFilter());
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(true);

        ChartManageService.Connect().PopulateInto(_sourceCache);
        AllChartsLoaded = true;

        AudioPlayerService.PlayingFileChanged += OnPlayingFileChanged;

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task TogglePlayAsync(ChartDto chart)
    {
        if (_playingChart == chart)
        {
            AudioPlayerService.Pause();
            _pausedChart = chart;
            SetPlayingChart(null);
            return;
        }

        if (_pausedChart == chart)
        {
            AudioPlayerService.Resume();
            _pausedChart = null;
            SetPlayingChart(chart);
            return;
        }

        if ((chart.DemoPath ?? chart.MusicPath) is not { } audioPath)
        {
            return;
        }

        _pausedChart = null;
        SetPlayingChart(chart);
        await Task.Run(() => AudioPlayerService.Play(audioPath)).ConfigureAwait(false);
    }

    private void OnPlayingFileChanged(object? sender, string? playingFilePath)
    {
        var playingChart = OwnedChart(playingFilePath);
        Dispatcher.UIThread.Post(() =>
        {
            SetPlayingChart(playingChart);
            _pausedChart = null;
        });
    }

    private void SetPlayingChart(ChartDto? chart)
    {
        if (_playingChart == chart)
        {
            return;
        }

        if (_playingChart is not null)
        {
            _playingChart.IsPlaying = false;
        }

        _playingChart = chart;

        if (chart is not null)
        {
            chart.IsPlaying = true;
        }
    }

    private ChartDto? OwnedChart(string? audioFilePath)
    {
        if (Path.GetDirectoryName(audioFilePath) is not { } folderPath)
        {
            return null;
        }

        return _sourceCache.Lookup(Path.GetFileName(folderPath)) is { HasValue: true, Value: var chart } && chart.FolderPath == folderPath
            ? chart
            : null;
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = null;
        ShowEasy = ShowHard = ShowMaster = ShowHidden = true;
        RatingMin = RatingLowerBound;
        RatingMax = RatingUpperBound;
        BpmMin = null;
        BpmMax = null;
        StreamerSafeOnly = false;
        HasVideoOnly = false;
    }

    [RelayCommand]
    private async Task RemoveChartAsync(ChartDto chart)
    {
        if (_playingChart == chart || _pausedChart == chart)
        {
            AudioPlayerService.Stop();
        }

        await ChartManageService.RemoveChartAsync(chart.FolderPath).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task UpdateAllChartsAsync(CancellationToken cancellationToken)
    {
        if (await ChartManageService.UpdateAllChartsAsync(cancellationToken).ConfigureAwait(true) is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Chart_UpdateAll_UpToDate);
        }
    }

    [RelayCommand]
    private async Task MigrateCustomAlbumsAsync(CancellationToken cancellationToken)
    {
        if (await ChartManageService.MigrateCustomAlbumsAsync(cancellationToken: cancellationToken).ConfigureAwait(true) is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Migration_None);
        }
    }

    [RelayCommand]
    private async Task ImportChartsAsync(IReadOnlyList<IStorageItem> files)
    {
        var paths = files.GetLocalPaths().OfType<string>().ToArray();
        if (paths is [])
        {
            return;
        }

        if (await ChartManageService.ImportChartsAsync(paths).ConfigureAwait(true))
        {
            SelectedChartSourceIndex = (int)ChartSource.Offline;
        }
    }

    private bool MatchesFilters(ChartDto chart)
    {
        var meta = chart.Manifest.Meta;

        if (chart.Source != (ChartSource)SelectedChartSourceIndex)
        {
            return false;
        }

        if (!SearchText.IsNullOrEmpty()
            && !meta.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            && !(meta.NameRomanized?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            && !meta.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            && !meta.Maps.Values.Any(m => m.Charters.Any(c => c.Contains(SearchText, StringComparison.OrdinalIgnoreCase))))
        {
            return false;
        }

        if (!(ShowEasy && ShowHard && ShowMaster && ShowHidden)
            && !((ShowEasy && chart.HasDifficulty(ChartDifficulty.Easy))
                 || (ShowHard && chart.HasDifficulty(ChartDifficulty.Hard))
                 || (ShowMaster && chart.HasDifficulty(ChartDifficulty.Master))
                 || (ShowHidden && chart.HasDifficulty(ChartDifficulty.Hidden))))
        {
            return false;
        }

        var ratingMin = RatingMin ?? RatingLowerBound;
        var ratingMax = RatingMax ?? RatingUpperBound;
        if ((ratingMin > RatingLowerBound || ratingMax < RatingUpperBound)
            && !meta.Maps.Values.Select(static m => (int)m.RatingValue).Any(r => r >= ratingMin && r <= ratingMax))
        {
            return false;
        }

        if ((BpmMin is { } bpmMin && meta.Bpm < bpmMin) || (BpmMax is { } bpmMax && meta.Bpm > bpmMax))
        {
            return false;
        }

        if (StreamerSafeOnly && !meta.SafeForStreamer)
        {
            return false;
        }

        if (HasVideoOnly && chart.VideoPath is null)
        {
            return false;
        }

        return true;
    }

    private Comparer<ChartDto> BuildComparer()
    {
        Comparison<ChartDto> comparison = SortField switch
        {
            ChartSortField.Author => (a, b) => string.Compare(a.Manifest.Meta.Author, b.Manifest.Meta.Author, StringComparison.OrdinalIgnoreCase),
            ChartSortField.Bpm => (a, b) => a.Manifest.Meta.Bpm.CompareTo(b.Manifest.Meta.Bpm),
            ChartSortField.Rating => (a, b) => a.MaxRating.CompareTo(b.MaxRating),
            ChartSortField.DateAdded => (a, b) => (a.Manifest.Meta.CreatedAt ?? 0).CompareTo(b.Manifest.Meta.CreatedAt ?? 0),
            ChartSortField.DateUpdated => (a, b) => (a.Manifest.Meta.UpdatedAt ?? 0).CompareTo(b.Manifest.Meta.UpdatedAt ?? 0),
            ChartSortField.DifficultyCount => (a, b) => a.Difficulties.Count.CompareTo(b.Difficulties.Count),
            ChartSortField.Size => (a, b) => a.SizeBytes.CompareTo(b.SizeBytes),
            _ => (a, b) => string.Compare(a.Manifest.Meta.Name, b.Manifest.Meta.Name, StringComparison.OrdinalIgnoreCase)
        };

        if (!SortDescending)
        {
            return Comparer<ChartDto>.Create(comparison);
        }

        var ascending = comparison;
        comparison = (a, b) => ascending(b, a);
        return Comparer<ChartDto>.Create(comparison);
    }

    private void RefreshFilter() => _sourceCache.Refresh();

    private void RefreshSort() => _comparer.OnNext(BuildComparer());

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(SearchText):
                _searchTextChanged.OnNext(Unit.Default);
                break;
            case nameof(SelectedChartSourceIndex) or nameof(ShowEasy) or nameof(ShowHard) or nameof(ShowMaster)
                or nameof(ShowHidden) or nameof(RatingMin) or nameof(RatingMax) or nameof(BpmMin) or nameof(BpmMax)
                or nameof(StreamerSafeOnly) or nameof(HasVideoOnly):
                RefreshFilter();
                break;
            case nameof(SortField) or nameof(SortDescending):
                RefreshSort();
                break;
        }
    }

    #region Injections

    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
