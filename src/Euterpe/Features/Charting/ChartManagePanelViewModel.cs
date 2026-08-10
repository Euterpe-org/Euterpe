using System.Collections.ObjectModel;
using Euterpe.Core.Proxies;

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

    [ObservableProperty]
    public partial bool AllChartsLoaded { get; set; }

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
        ObserveSelection();
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

    #region Injections

    public required PlaybackState Playback { get; init; }
    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IGameShareService GameShareService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }
    public required ProgressDialogService ProgressDialogService { get; init; }
    public required ShareImportDialogService ShareImportDialogService { get; init; }
    public required TopLevelProxy TopLevel { get; init; }

    #endregion Injections
}
