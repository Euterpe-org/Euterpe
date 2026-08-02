using System.Collections.ObjectModel;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Euterpe.Core.Proxies;
using Euterpe.Models.Progress;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ChartManageItemViewModel> _charts;
    private readonly SourceCache<ChartManageItemViewModel, string> _sourceCache = new(x => x.Chart.FolderPath);

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

    [ObservableProperty]
    public partial bool IsSelectionMode { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(SelectedCountDisplay))]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial bool CanShareSelection { get; set; }

    public bool HasSelection => SelectedCount > 0;
    public string SelectedCountDisplay => string.Format(CultureInfo.CurrentCulture, XAML.ChartManage_SelectedCount, SelectedCount);

    public ChartFilterViewModel Filter { get; } = new();
    public ReadOnlyObservableCollection<ChartManageItemViewModel> Charts => _charts;

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
            .Filter(item => Filter.Matches(item.Chart))
            .SortAndBindOnUI(out _charts, comparer.AsSystemObservable())
            .Subscribe();

        Filter.Changed.Subscribe(this, static (_, vm) => vm._sourceCache.Refresh());

        _sourceCache.Connect()
            .AutoRefresh(static item => item.IsSelected)
            .Filter(static item => item.IsSelected)
            .ToCollection()
            .Subscribe(selectedItems =>
            {
                SelectedCount = selectedItems.Count;
                CanShareSelection = selectedItems.Count is > 0 and <= GameSharePackage.MaximumChartCount
                                    && selectedItems.All(static item => item.CanShare);
            });
    }

    partial void OnIsSelectionModeChanged(bool value)
    {
        if (value)
        {
            return;
        }

        foreach (var item in _sourceCache.Items)
        {
            item.IsSelected = false;
        }
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(true);

        ChartManageService.Connect()
            .Transform(static chart => new ChartManageItemViewModel(chart))
            .PopulateInto(_sourceCache);
        AllChartsLoaded = true;

        Logger.LogInformation("{ViewModel} Initialized", nameof(ChartManagePanelViewModel));
    }

    [RelayCommand]
    private async Task ActivateChartAsync(ChartManageItemViewModel item)
    {
        if (IsSelectionMode)
        {
            item.IsSelected = !item.IsSelected;
            return;
        }

        await TogglePlayAsync(item.Chart).ConfigureAwait(false);
    }

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
        RunWithProgressDialogAsync(ChartManage_Migrating, ChartManage_MigratingHint, false, async progress =>
        {
            var migratedCount = await ChartManageService.MigrateCustomAlbumsAsync(progress).ConfigureAwait(true);
            if (migratedCount is 0)
            {
                NotificationService.NoticeLight(Notification_Content_Migration_None);
            }
        });

    public Task DownloadChartAsync(string cid) =>
        RunWithProgressDialogAsync(ChartManage_Downloading, ChartManage_DownloadingHint, true, progress =>
            ChartManageService.DownloadChartAsync(cid, progress));

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

    [RelayCommand]
    private Task ImportShareAsync() => ShareImportDialogService.ShowAsync();

    [RelayCommand]
    private async Task ShareSelectedAsync()
    {
        if (!CanShareSelection)
        {
            return;
        }

        var cids = _sourceCache.Items
            .Where(static item => item.IsSelected)
            .Select(static item => item.Chart.Manifest.Cid!.Value)
            .ToArray();

        var shareLink = GameShareService.CreateChartShareLink(cids);
        await TopLevel.Clipboard!.SetTextAsync(shareLink).ConfigureAwait(true);
        NotificationService.SuccessLight(Notification_Content_Share_Copy_Success);
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var folderPaths = _sourceCache.Items
            .Where(static item => item.IsSelected)
            .Select(static item => item.Chart.FolderPath)
            .ToArray();
        if (folderPaths is [])
        {
            return;
        }

        if (await MessageBoxService.WarningConfirmAsync(MessageBox_Content_Chart_BulkDelete_Confirm, folderPaths.Length).ConfigureAwait(true) is not MessageBoxResult.Yes)
        {
            return;
        }

        if (Playback.PlayingKey is { } playingKey && folderPaths.Contains(playingKey))
        {
            AudioPlayerService.Stop();
        }

        await ChartManageService.DeleteChartsAsync(folderPaths).ConfigureAwait(true);
        IsSelectionMode = false;
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
        var dialog = DialogService.ShowOverlayAsync<ProgressDialog, ProgressDialogViewModel>(
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

    private Comparer<ChartManageItemViewModel> BuildComparer()
    {
        var comparison = SortField switch
        {
            ChartSortField.Author => ByText(x => x.Chart.Manifest.Meta.Author),
            ChartSortField.Bpm => By(x => x.Chart.Manifest.Meta.Bpm),
            ChartSortField.Rating => By(x => x.Chart.MaxRating),
            ChartSortField.DateAdded => By(x => x.Chart.Manifest.Meta.CreatedAt ?? 0),
            ChartSortField.DateUpdated => By(x => x.Chart.Manifest.Meta.UpdatedAt ?? 0),
            ChartSortField.MapCount => By(x => x.Chart.Difficulties.Count),
            ChartSortField.Size => By(x => x.Chart.SizeBytes),
            _ => ByText(x => x.Chart.Manifest.Meta.Name)
        };

        return Comparer<ChartManageItemViewModel>.Create(SortDescending ? (a, b) => comparison(b, a) : comparison);

        static Comparison<ChartManageItemViewModel> By<TKey>(Func<ChartManageItemViewModel, TKey> key) where TKey : IComparable<TKey>
        {
            return (a, b) => key(a).CompareTo(key(b));
        }

        static Comparison<ChartManageItemViewModel> ByText(Func<ChartManageItemViewModel, string> key)
        {
            return (a, b) => string.Compare(key(a), key(b), StringComparison.OrdinalIgnoreCase);
        }
    }

    #region Injections

    public required PlaybackState Playback { get; init; }
    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IDialogService DialogService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IGameShareService GameShareService { get; init; }
    public required GameSwitcher GameSwitcher { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }
    public required ProgressDialogViewModel ProgressDialogViewModel { get; init; }
    public required ShareImportDialogService ShareImportDialogService { get; init; }
    public required TopLevelProxy TopLevel { get; init; }

    #endregion Injections
}
