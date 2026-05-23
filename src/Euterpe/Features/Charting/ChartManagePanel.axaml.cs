namespace Euterpe.Features.Charting;

[Route("/charting/manage", DisplayName = Panel_Charting_ChartManage, Order = 0)]
[PerGameView]
public sealed partial class ChartManagePanel : UserControl
{
    public ChartManagePanel()
    {
        InitializeComponent();
    }
}