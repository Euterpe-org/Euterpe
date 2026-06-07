using System.Collections.ObjectModel;

namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGame]
public sealed partial class ChartManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<ChartDto> _charts;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderName);
    private ChartSource _selectedSource = ChartSource.Online;

    [ObservableProperty]
    public partial int SelectedChartSourceIndex { get; set; }

    public ReadOnlyObservableCollection<ChartDto> Charts => _charts;

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    public ChartManagePanelViewModel()
    {
        _sourceCache.Connect()
            .Filter(x => SearchText.IsNullOrEmpty()
                         || x.Manifest.Meta.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .Filter(x => _selectedSource is not ChartSource.Online || x.Source is ChartSource.Online)
            .Filter(x => _selectedSource is not ChartSource.Offline || x.Source is ChartSource.Offline)
            .Bind(out _charts)
            .Subscribe();
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);

        ChartManageService.Connect().PopulateInto(_sourceCache);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    partial void OnSelectedChartSourceIndexChanged(int value)
    {
        _selectedSource = (ChartSource)value;
        _sourceCache.Refresh();
    }

    [UsedImplicitly]
    partial void OnSearchTextChanged(string? value) => _sourceCache.Refresh();

    [RelayCommand]
    private async Task PlayAsync(ChartDto chart)
    {
        if (chart.MusicPath is not { } musicPath)
        {
            return;
        }

        await Task.Run(() => AudioPlayerService.Play(musicPath)).ConfigureAwait(false);
    }

    [RelayCommand]
    private void StopMusic() => AudioPlayerService.Stop();

    #region Injections

    public required IAudioPlayerService AudioPlayerService { get; init; }
    public required IChartManageService ChartManageService { get; init; }
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}