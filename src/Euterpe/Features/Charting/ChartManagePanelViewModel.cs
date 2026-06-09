using System.Collections.ObjectModel;
using DynamicData.Binding;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGame]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private const int BpmCeiling = 999;
    private const double RatingCeiling = 99;

    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly ReadOnlyObservableCollection<string> _scenes;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderName);
    private readonly BehaviorSubject<IComparer<ChartDto>> _comparer;

    // FolderName of the chart whose preview is paused with its player still loaded (null = none).
    private string? _pausedFolder;

    // Source filter (Online / Offline)
    [ObservableProperty]
    public partial int SelectedChartSourceIndex { get; set; }

    // Free-text search (name / romanized / author / charter)
    [ObservableProperty]
    public partial string? SearchText { get; set; }

    // Difficulty presence
    [ObservableProperty] public partial bool ShowEasy { get; set; } = true;
    [ObservableProperty] public partial bool ShowHard { get; set; } = true;
    [ObservableProperty] public partial bool ShowMaster { get; set; } = true;
    [ObservableProperty] public partial bool ShowHidden { get; set; } = true;

    // Rating range (across maps)
    [ObservableProperty] public partial double RatingMin { get; set; }
    [ObservableProperty] public partial double RatingMax { get; set; } = RatingCeiling;

    // BPM range
    [ObservableProperty] public partial int BpmMin { get; set; }
    [ObservableProperty] public partial int BpmMax { get; set; } = BpmCeiling;

    [ObservableProperty] public partial bool StreamerSafeOnly { get; set; }
    [ObservableProperty] public partial bool HasVideoOnly { get; set; }

    [ObservableProperty] public partial string? SelectedScene { get; set; }

    // Sort
    [ObservableProperty] public partial ChartSortField SortField { get; set; }
    [ObservableProperty] public partial bool SortDescending { get; set; }

    // FolderName of the chart whose preview is currently playing (null = none)
    [ObservableProperty] public partial string? CurrentlyPlaying { get; set; }

    public ReadOnlyObservableCollection<ChartDto> Charts => _charts;
    public ReadOnlyObservableCollection<string> Scenes => _scenes;

    public ChartManagePanelViewModel()
    {
        _comparer = new(BuildComparer());

        var connect = _sourceCache.Connect();

        connect
            .Filter(MatchesFilters)
            .SortAndBind(out _charts, _comparer.AsSystemObservable())
            .Subscribe();

        connect
            .DistinctValues(x => x.Manifest.Meta.Scene)
            .SortAndBind(out _scenes, SortExpressionComparer<string>.Ascending(x => x))
            .Subscribe();
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);

        ChartManageService.Connect().PopulateInto(_sourceCache);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task TogglePlayAsync(ChartDto chart)
    {
        var folder = chart.FolderName;

        // Pause the chart that is currently playing.
        if (CurrentlyPlaying == folder)
        {
            AudioPlayerService.Pause();
            _pausedFolder = folder;
            CurrentlyPlaying = null;
            return;
        }

        // Resume this chart if it was paused and nothing else has played since.
        if (_pausedFolder == folder)
        {
            AudioPlayerService.Resume();
            _pausedFolder = null;
            CurrentlyPlaying = folder;
            return;
        }

        if ((chart.DemoPath ?? chart.MusicPath) is not { } audioPath)
        {
            return;
        }

        // Reflect playback immediately (on the UI thread), then decode/start off-thread.
        // Play() stops and disposes any current/paused player first, so only one preview ever exists.
        _pausedFolder = null;
        CurrentlyPlaying = folder;
        await Task.Run(() => AudioPlayerService.Play(audioPath)).ConfigureAwait(false);
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = null;
        ShowEasy = ShowHard = ShowMaster = ShowHidden = true;
        RatingMin = 0;
        RatingMax = RatingCeiling;
        BpmMin = 0;
        BpmMax = BpmCeiling;
        StreamerSafeOnly = false;
        HasVideoOnly = false;
        SelectedScene = null;
    }

    private bool MatchesFilters(ChartDto chart)
    {
        var meta = chart.Manifest.Meta;
        var source = (ChartSource)SelectedChartSourceIndex;

        if (source is ChartSource.Online && chart.Source is not ChartSource.Online)
        {
            return false;
        }

        if (source is ChartSource.Offline && chart.Source is not ChartSource.Offline)
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

        if ((RatingMin > 0 || RatingMax < RatingCeiling)
            && !meta.Maps.Values.Any(m => ChartRating.Parse(m.Rating) is var r && r >= RatingMin && r <= RatingMax))
        {
            return false;
        }

        if ((BpmMin > 0 || BpmMax < BpmCeiling) && (meta.Bpm < BpmMin || meta.Bpm > BpmMax))
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

        if (!SelectedScene.IsNullOrEmpty() && !string.Equals(meta.Scene, SelectedScene, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private IComparer<ChartDto> BuildComparer()
    {
        Comparison<ChartDto> comparison = SortField switch
        {
            ChartSortField.Author => (a, b) => string.Compare(a.Manifest.Meta.Author, b.Manifest.Meta.Author, StringComparison.OrdinalIgnoreCase),
            ChartSortField.Bpm => (a, b) => a.Manifest.Meta.Bpm.CompareTo(b.Manifest.Meta.Bpm),
            ChartSortField.Rating => (a, b) => ChartRating.Max(a).CompareTo(ChartRating.Max(b)),
            ChartSortField.DateAdded => (a, b) => (a.Manifest.Meta.CreatedAt ?? 0).CompareTo(b.Manifest.Meta.CreatedAt ?? 0),
            ChartSortField.DateUpdated => (a, b) => (a.Manifest.Meta.UpdatedAt ?? 0).CompareTo(b.Manifest.Meta.UpdatedAt ?? 0),
            ChartSortField.DifficultyCount => (a, b) => a.Difficulties.Count.CompareTo(b.Difficulties.Count),
            _ => (a, b) => string.Compare(a.Manifest.Meta.Name, b.Manifest.Meta.Name, StringComparison.OrdinalIgnoreCase)
        };

        if (SortDescending)
        {
            var ascending = comparison;
            comparison = (a, b) => ascending(b, a);
        }

        return Comparer<ChartDto>.Create(comparison);
    }

    private void RefreshFilter() => _sourceCache.Refresh();

    private void RefreshSort() => _comparer.OnNext(BuildComparer());

    partial void OnSelectedChartSourceIndexChanged(int value) => RefreshFilter();
    partial void OnSearchTextChanged(string? value) => RefreshFilter();
    partial void OnShowEasyChanged(bool value) => RefreshFilter();
    partial void OnShowHardChanged(bool value) => RefreshFilter();
    partial void OnShowMasterChanged(bool value) => RefreshFilter();
    partial void OnShowHiddenChanged(bool value) => RefreshFilter();
    partial void OnRatingMinChanged(double value) => RefreshFilter();
    partial void OnRatingMaxChanged(double value) => RefreshFilter();
    partial void OnBpmMinChanged(int value) => RefreshFilter();
    partial void OnBpmMaxChanged(int value) => RefreshFilter();
    partial void OnStreamerSafeOnlyChanged(bool value) => RefreshFilter();
    partial void OnHasVideoOnlyChanged(bool value) => RefreshFilter();
    partial void OnSelectedSceneChanged(string? value) => RefreshFilter();
    partial void OnSortFieldChanged(ChartSortField value) => RefreshSort();
    partial void OnSortDescendingChanged(bool value) => RefreshSort();

    #region Injections

    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}
