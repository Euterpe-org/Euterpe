namespace Euterpe.Features.Charting;

[Route("/charting", DisplayName = Page_Charting, Icon = "Music", Order = 2)]
[PerGameView]
public sealed partial class ChartingPage : UserControl
{
    public ChartingPage()
    {
        InitializeComponent();
    }
}