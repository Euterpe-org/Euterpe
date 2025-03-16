using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Panels.Charting;

public sealed class ChartManagePanelViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<Chart> _charts;
    private readonly SourceCache<Chart, string> _sourceCache = new(x => x.Title);
    public ReadOnlyObservableCollection<Chart> Charts => _charts;

    public ChartManagePanelViewModel()
    {
        _sourceCache.Connect()
            .Bind(out _charts)
            .Subscribe();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        await ChartManageService.InitializeChartsAsync(_sourceCache).ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(ChartManagePanelViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required IChartManageService ChartManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<ChartManagePanelViewModel> Logger { get; init; }

    #endregion Injections
}