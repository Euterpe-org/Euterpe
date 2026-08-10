namespace Euterpe.Features.Charting;

public sealed partial class ChartManageItemViewModel(ChartDto chart) : ObservableObject
{
    public ChartDto Chart { get; } = chart;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
