namespace Euterpe.Features.Setting;

[Route("/setting/appearance", DisplayName = Panel_Setting_Appearance, Order = 1)]
public sealed partial class AppearancePanel : UserControl
{
    public AppearancePanel()
    {
        InitializeComponent();
    }
}