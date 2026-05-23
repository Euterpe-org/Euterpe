namespace Euterpe.Features.Setting;

[Route("/setting/about", DisplayName = Panel_Setting_About, Order = 0)]
public sealed partial class AboutPanel : UserControl
{
    public AboutPanel()
    {
        InitializeComponent();
    }
}