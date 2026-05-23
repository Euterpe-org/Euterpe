namespace Euterpe.Features.Modding;

[Route("/modding/develop", DisplayName = Panel_Modding_ModDevelop, Order = 2)]
[PerGameView]
public sealed partial class ModDevelopPanel : UserControl
{
    public ModDevelopPanel()
    {
        InitializeComponent();
    }
}