namespace MuseDashModToolsUI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    [ObservableProperty]
    private bool _isCollapsed;

    public static string Version => $"v{AppVersion}";

    public static PageNavItem[] PageNavItems =>
    [
        new() { Name = "Mod Manage" },
        new() { Name = "Setting" }
    ];
}