namespace Euterpe.Views.Panels.Logging;

[Route("/logging/app", DisplayName = Panel_Logging_AppLog, Order = 0)]
public sealed partial class AppLogPanel : UserControl
{
    public AppLogPanel()
    {
        InitializeComponent();
        LogListBox.Loaded += (_, _) => LogListBox.ScrollIntoView(LogListBox.ItemCount - 1);
    }
}