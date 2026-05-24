namespace Euterpe.Features.Setting;

[Route("/setting/experience", DisplayName = Panel_Setting_Experience, Order = 2)]
public sealed partial class ExperiencePanel : UserControl
{
    public ExperiencePanel()
    {
        InitializeComponent();
    }
}