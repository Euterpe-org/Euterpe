namespace Euterpe.Views.Windows;

public sealed partial class MainSplashWindow : SplashWindow
{
    public MainSplashWindow()
    {
        InitializeComponent();
    }

    protected override async Task<bool> CanClose()
    {
        if (DataContext is MainSplashWindowViewModel vm)
        {
            await vm.Ready.WaitAsync().ConfigureAwait(true);
        }

        return true;
    }

    protected override async Task<Window?> CreateNextWindow() => IocContainer.Resolve<MainWindow>();
}