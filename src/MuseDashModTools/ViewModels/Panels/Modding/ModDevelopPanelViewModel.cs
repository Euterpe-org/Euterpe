namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModDevelopPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallModTemplateCommand))]
    public partial bool DotNetSdkInstalled { get; set; }

    [ObservableProperty]
    public partial bool ModTemplateInstalled { get; set; }

    public override async Task InitializeAsync()
    {
        DotNetSdkInstalled = await LocalService.CheckDotNetSdkInstalledAsync().ConfigureAwait(true);
        ModTemplateInstalled = await LocalService.CheckModTemplateInstalledAsync().ConfigureAwait(true);

        Logger.ZLogInformation($"{nameof(ModDevelopPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallDotNetSdkAsync()
    {
        Logger.ZLogInformation($"Installing DotNet SDK...");
        var success = await PlatformService.InstallDotNetSdkAsync().ConfigureAwait(false);
        if (!success)
        {
            await MessageBoxService.ErrorAsync("Failed to install DotNet SDK").ConfigureAwait(false);
        }

        Logger.ZLogInformation($"DotNet SDK installed successfully");
        DotNetSdkInstalled = true;
    }

    [RelayCommand(CanExecute = nameof(DotNetSdkInstalled))]
    private async Task InstallModTemplateAsync()
    {
        Logger.ZLogInformation($"Installing Mod Template...");
        try
        {
            await PlatformService.InstallModTemplateAsync().ConfigureAwait(false);
            Logger.ZLogInformation($"Mod Template installed successfully");
            ModTemplateInstalled = true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install Mod Template");
            await MessageBoxService.ErrorAsync("Failed to install Mod Template").ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<ModDevelopPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}