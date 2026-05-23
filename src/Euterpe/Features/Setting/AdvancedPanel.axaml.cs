namespace Euterpe.Features.Setting;

[Route("/setting/advanced", DisplayName = Panel_Setting_Advanced, Order = 5)]
public sealed partial class AdvancedPanel : UserControl
{
    public AdvancedPanel()
    {
        InitializeComponent();
    }
}