namespace Euterpe.Views.Panels.Modding;

[Route("/modding/melonloader", DisplayName = Panel_Modding_MelonLoader, Order = 1)]
[PerGameView]
public sealed partial class MelonLoaderPanel : UserControl
{
    public MelonLoaderPanel()
    {
        InitializeComponent();
    }
}