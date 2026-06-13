using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGame]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderName);
    private ChartDto? _pausedChart;
    private ChartDto? _playingChart;

    public static IReadOnlyList<EnumOption<ChartSource>> ChartSources { get; } =
        [.. ChartSourceExtensions.GetValues().Select(static source =>
            new EnumOption<ChartSource>(source, $"{nameof(ChartSource)}_{source.ToStringFast()}"))];

    public static IReadOnlyList<EnumOption<ChartSortField>> SortFields { get; } =
    [
        .. ChartSortFieldExtensions.GetValues().Select(static field =>
            new EnumOption<ChartSortField>(field, $"{nameof(ChartSortField)}_{field.ToStringFast()}"))
    ];

    [ObservableProperty]
    public partial ChartSortField SortField { get; set; }

    [ObservableProperty]
    public partial bool SortDescending { get; set; }

    [ObservableProperty]
    public partial bool AllChartsLoaded { get; set; }

    public ChartFilterViewModel Filter { get; } = new();
    public ReadOnlyObservableCollection<ChartDto> Charts => _charts;

    public ChartManagePanelViewModel()
    {
        var comparer = new[]
            {
                this.ObservePropertyChanged(static x => x.SortField).AsUnitObservable(),
                this.ObservePropertyChanged(static x => x.SortDescending).AsUnitObservable()
            }
            .Merge()
            .Select(this, static (_, vm) => vm.BuildComparer());

        _sourceCache.Connect()
            .Filter(chart => Filter.Matches(chart))
            .SortAndBind(out _charts, comparer.AsSystemObservable())
            .Subscribe();

        Filter.Changed.Subscribe(this, static (_, vm) => vm._sourceCache.Refresh());
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

        if (chart.AudioPath is not { } audioPath)
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
    private void ResetFilters() => Filter.Reset();

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
            Filter.Source = ChartSource.Offline;
        }
    }

    private Comparer<ChartDto> BuildComparer()
    {
        var comparison = SortField switch
        {
            ChartSortField.Author => ByText(x => x.Manifest.Meta.Author),
            ChartSortField.Bpm => By(x => x.Manifest.Meta.Bpm),
            ChartSortField.Rating => By(x => x.MaxRating),
            ChartSortField.DateAdded => By(x => x.Manifest.Meta.CreatedAt ?? 0),
            ChartSortField.DateUpdated => By(x => x.Manifest.Meta.UpdatedAt ?? 0),
            ChartSortField.MapCount => By(x => x.Difficulties.Count),
            ChartSortField.Size => By(x => x.SizeBytes),
            _ => ByText(x => x.Manifest.Meta.Name)
        };

        return Comparer<ChartDto>.Create(SortDescending ? (a, b) => comparison(b, a) : comparison);

        static Comparison<ChartDto> By<TKey>(Func<ChartDto, TKey> key) where TKey : IComparable<TKey>
        {
            return (a, b) => key(a).CompareTo(key(b));
        }

        static Comparison<ChartDto> ByText(Func<ChartDto, string> key)
        {
            return (a, b) => string.Compare(key(a), key(b), StringComparison.OrdinalIgnoreCase);
        }
    }

    #region Injections

    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
