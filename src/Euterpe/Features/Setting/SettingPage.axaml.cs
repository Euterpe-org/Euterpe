namespace Euterpe.Features.Setting;

[Route("/setting", DisplayName = Page_Setting, Icon = "Setting", Order = 4)]
public sealed partial class SettingPage : UserControl
{
    public SettingPage()
    {
        InitializeComponent();
    }
}