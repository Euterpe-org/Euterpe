namespace Euterpe.Features.Charting;

public sealed partial class ChartManageItemViewModel(ChartDto chart) : ObservableObject
{
    public ChartDto Chart { get; } = chart;

    public bool CanShare => Chart is { IsOnline: true, Manifest.Cid: not null };

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
