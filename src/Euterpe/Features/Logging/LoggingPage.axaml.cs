namespace Euterpe.Features.Logging;

[PerGameView]
[Route("/logging", DisplayName = Page_Logging, Icon = "Terminal", Order = 3)]
public sealed partial class LoggingPage : UserControl
{
    public LoggingPage()
    {
        InitializeComponent();
    }
}