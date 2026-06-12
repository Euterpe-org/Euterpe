using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Shell;

public sealed partial class MainWindow : UrsaWindow
{
    public WindowNotificationManager Notifier { get; }

    public MainWindow()
    {
        InitializeComponent();

        Notifier = new WindowNotificationManager(this);
    }
}
