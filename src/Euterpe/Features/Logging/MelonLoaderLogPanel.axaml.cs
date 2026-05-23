namespace Euterpe.Features.Logging;

[Route("/logging/melonloader", DisplayName = Panel_Logging_MelonLoaderLog, Order = 1)]
public sealed partial class MelonLoaderLogPanel : UserControl
{
    public MelonLoaderLogPanel()
    {
        InitializeComponent();
    }
}