namespace MuseDashModTools.Views.Components;

public partial class WizardWindow : SplashWindow
{
    public WizardWindow()
    {
        InitializeComponent();
    }

    protected override Task<Window?> CreateNextWindow() =>
        Task.FromResult<Window?>(App.Container.Resolve<MainWindow>());
}