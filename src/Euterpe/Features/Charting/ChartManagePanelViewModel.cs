using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGame]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private const int MinRating = 1;
    private const int MaxRating = 12;

    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderName);
    private readonly BehaviorSubject<IComparer<ChartDto>> _comparer;

    private string? _pausedFolderName;

    [ObservableProperty]
    public partial int SelectedChartSourceIndex { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty] public partial bool ShowEasy { get; set; } = true;
    [ObservableProperty] public partial bool ShowHard { get; set; } = true;
    [ObservableProperty] public partial bool ShowMaster { get; set; } = true;
    [ObservableProperty] public partial bool ShowHidden { get; set; } = true;

    [ObservableProperty] public partial int? RatingMin { get; set; } = MinRating;
    [ObservableProperty] public partial int? RatingMax { get; set; } = MaxRating;

    [ObservableProperty] public partial int? BpmMin { get; set; }
    [ObservableProperty] public partial int? BpmMax { get; set; }

    [ObservableProperty] public partial bool StreamerSafeOnly { get; set; }
    [ObservableProperty] public partial bool HasVideoOnly { get; set; }

    [ObservableProperty] public partial ChartSortField SortField { get; set; }
    [ObservableProperty] public partial bool SortDescending { get; set; }

    [ObservableProperty] public partial string? PlayingFolderName { get; set; }

    [ObservableProperty] public partial bool AllChartsLoaded { get; set; }

    public ReadOnlyObservableCollection<ChartDto> Charts => _charts;

    public ChartManagePanelViewModel()
    {
        _comparer = new(BuildComparer());

        var connect = _sourceCache.Connect();

        connect
            .Filter(MatchesFilters)
            .SortAndBind(out _charts, _comparer.AsSystemObservable())
            .Subscribe();
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ChartManageService.Connect().PopulateInto(_sourceCache);
            AllChartsLoaded = true;
        });

        AudioPlayerService.PlaybackEnded += OnPlaybackEnded;

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task TogglePlayAsync(ChartDto chart)
    {
        var folderName = chart.FolderName;

        if (PlayingFolderName == folderName)
        {
            AudioPlayerService.Pause();
            _pausedFolderName = folderName;
            PlayingFolderName = null;
            return;
        }

        if (_pausedFolderName == folderName)
        {
            AudioPlayerService.Resume();
            _pausedFolderName = null;
            PlayingFolderName = folderName;
            return;
        }

        if ((chart.DemoPath ?? chart.MusicPath) is not { } audioPath)
        {
            return;
        }

        _pausedFolderName = null;
        PlayingFolderName = folderName;
        await Task.Run(() => AudioPlayerService.Play(audioPath)).ConfigureAwait(false);
    }

    // Fires on the audio thread.
    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        var endedFolderName = PlayingFolderName;
        Dispatcher.UIThread.Post(() =>
        {
            if (PlayingFolderName == endedFolderName)
            {
                PlayingFolderName = null;
            }
        });
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = null;
        ShowEasy = ShowHard = ShowMaster = ShowHidden = true;
        RatingMin = MinRating;
        RatingMax = MaxRating;
        BpmMin = null;
        BpmMax = null;
        StreamerSafeOnly = false;
        HasVideoOnly = false;
    }

    [RelayCommand]
    private async Task RemoveChartAsync(ChartDto chart)
    {
        // Release the audio file handle before the folder is deleted.
        if (PlayingFolderName == chart.FolderName || _pausedFolderName == chart.FolderName)
        {
            AudioPlayerService.Stop();
            PlayingFolderName = null;
            _pausedFolderName = null;
        }

        await ChartManageService.RemoveChartAsync(chart.FolderPath).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task UpdateAllChartsAsync(CancellationToken cancellationToken)
    {
        if (await ChartManageService.UpdateAllChartsAsync(cancellationToken).ConfigureAwait(false) is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Chart_UpdateAll_UpToDate);
        }
    }

    [RelayCommand]
    private async Task MigrateCustomAlbumsAsync(CancellationToken cancellationToken)
    {
        if (await ChartManageService.MigrateCustomAlbumsAsync(cancellationToken: cancellationToken).ConfigureAwait(false) is 0)
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

        if (await ChartManageService.ImportChartsAsync(paths).ConfigureAwait(false))
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

        var ratingMin = RatingMin ?? MinRating;
        var ratingMax = RatingMax ?? MaxRating;
        if ((ratingMin > MinRating || ratingMax < MaxRating)
            && !meta.Maps.Values.Any(m =>
                (int)Math.Floor(ChartRating.Parse(m.Rating)) is var r && r >= ratingMin && r <= ratingMax))
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
            ChartSortField.Size => (a, b) => ChartRating.Size(a).CompareTo(ChartRating.Size(b)),
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
    partial void OnRatingMinChanged(int? value) => RefreshFilter();
    partial void OnRatingMaxChanged(int? value) => RefreshFilter();
    partial void OnBpmMinChanged(int? value) => RefreshFilter();
    partial void OnBpmMaxChanged(int? value) => RefreshFilter();
    partial void OnStreamerSafeOnlyChanged(bool value) => RefreshFilter();
    partial void OnHasVideoOnlyChanged(bool value) => RefreshFilter();
    partial void OnSortFieldChanged(ChartSortField value) => RefreshSort();
    partial void OnSortDescendingChanged(bool value) => RefreshSort();

    #region Injections

    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
