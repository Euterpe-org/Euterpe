namespace Euterpe.Views.Pages;

[Route("/logging", DisplayName = Page_Logging, Icon = "Terminal", Order = 3)]
public sealed partial class LoggingPage : UserControl
{
    public LoggingPage()
    {
        InitializeComponent();
    }
}