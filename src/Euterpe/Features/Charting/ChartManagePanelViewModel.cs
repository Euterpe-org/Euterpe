using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using Euterpe.Models.Progress;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderPath);

    public static IReadOnlyList<EnumOption<ChartSource>> ChartSources { get; } =
    [
        .. ChartSourceExtensions.GetValues().Select(static source =>
            new EnumOption<ChartSource>(source, $"{nameof(ChartSource)}_{source.ToStringFast()}"))
    ];

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
            .SortAndBindOnUI(out _charts, comparer.AsSystemObservable())
            .Subscribe();

        Filter.Changed.Subscribe(this, static (_, vm) => vm._sourceCache.Refresh());
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(true);

        ChartManageService.Connect().PopulateInto(_sourceCache);
        AllChartsLoaded = true;

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task TogglePlayAsync(ChartDto chart)
    {
        if (Playback.PlayingKey == chart.FolderPath)
        {
            if (Playback.Status is PlaybackStatus.Playing)
            {
                AudioPlayerService.Pause();
            }
            else
            {
                AudioPlayerService.Resume();
            }

            return;
        }

        if (chart.AudioPath is { } audioPath)
        {
            await AudioPlayerService.PlayAsync(chart.FolderPath, audioPath).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private void ResetFilters() => Filter.Reset();

    [RelayCommand]
    private async Task RemoveChartAsync(ChartDto chart)
    {
        if (Playback.PlayingKey == chart.FolderPath)
        {
            AudioPlayerService.Stop();
        }

        await ChartManageService.RemoveChartAsync(chart.FolderPath).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task UpdateAllChartsAsync(CancellationToken cancellationToken)
    {
        var updatedCount = await ChartManageService.UpdateAllChartsAsync(cancellationToken).ConfigureAwait(true);
        if (updatedCount is 0)
        {
            NotificationService.NoticeLight(Notification_Content_Chart_UpdateAll_UpToDate);
        }
    }

    [RelayCommand]
    public Task MigrateCustomAlbumsAsync() =>
        RunWithProgressDialogAsync(ChartManage_Migrating, ChartManage_MigratingHint, indeterminate: false, async progress =>
        {
            var migratedCount = await ChartManageService.MigrateCustomAlbumsAsync(progress).ConfigureAwait(true);
            if (migratedCount is 0)
            {
                NotificationService.NoticeLight(Notification_Content_Migration_None);
            }
        });

    public Task DownloadChartAsync(string chartId) =>
        RunWithProgressDialogAsync(ChartManage_Downloading, ChartManage_DownloadingHint, indeterminate: true, progress =>
            ChartManageService.DownloadChartAsync(chartId, progress));

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

    private async Task RunWithProgressDialogAsync(string title, string hint, bool indeterminate, Func<IProgress<BatchProgress>, Task> work)
    {
        ProgressDialogViewModel.Reset();
        ProgressDialogViewModel.IsIndeterminate = indeterminate;
        ProgressDialogViewModel.Hint = hint;

        var options = new OverlayDialogOptions
        {
            Title = title,
            CanDragMove = false,
            CanResize = false,
            IsCloseButtonVisible = false
        };

        GameSwitcher.CanSwitch = false;
        var dialog = DialogService.ShowOverlayAsync<ProgressDialog, ProgressDialogViewModel, object>(
            ProgressDialogViewModel, options, MainWindowViewModel.DialogHostId);
        try
        {
            var progress = new Progress<BatchProgress>(ProgressDialogViewModel.Report);
            await work(progress).ConfigureAwait(true);
        }
        finally
        {
            ProgressDialogViewModel.Close();
            GameSwitcher.CanSwitch = true;
            await dialog.ConfigureAwait(true);
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

    public required PlaybackState Playback { get; init; }
    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IDialogService DialogService { get; init; }
    public required GameSwitcher GameSwitcher { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }
    public required ProgressDialogViewModel ProgressDialogViewModel { get; init; }

    #endregion Injections
}
