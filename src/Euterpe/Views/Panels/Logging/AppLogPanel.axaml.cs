namespace Euterpe.Views.Panels.Logging;

public sealed partial class AppLogPanel : UserControl
{
    public AppLogPanel()
    {
        InitializeComponent();
        LogListBox.Loaded += (_, _) =>
        {
            if (LogListBox.ItemCount > 0)
                LogListBox.ScrollIntoView(LogListBox.ItemCount - 1);
        };
    }
}