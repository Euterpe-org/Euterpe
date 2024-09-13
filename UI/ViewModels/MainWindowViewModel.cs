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
    [RelayCommand]
    private void AttachSavingOnExitEvent()
    {
        GetCurrentDesktop()!.Exit += (_, _) => SavingService.SaveSettingAsync();
    }

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; } = null!;

    #endregion Injections
}