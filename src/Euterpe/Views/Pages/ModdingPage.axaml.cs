namespace Euterpe.Views.Pages;

[Route("/modding", DisplayName = Page_Modding, Icon = "Wrench", Order = 1)]
[PerGameView]
public sealed partial class ModdingPage : UserControl
{
    public ModdingPage()
    {
        InitializeComponent();
    }
}