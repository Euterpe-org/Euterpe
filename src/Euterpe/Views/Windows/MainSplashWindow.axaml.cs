namespace Euterpe.Views.Windows;

public sealed partial class MainSplashWindow : SplashWindow
{
    public MainSplashWindow()
    {
        InitializeComponent();
    }

    protected override async Task<Window?> CreateNextWindow() => IocContainer.Resolve<MainWindow>();
}