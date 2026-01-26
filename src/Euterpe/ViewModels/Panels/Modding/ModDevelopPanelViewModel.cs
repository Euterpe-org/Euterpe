namespace Euterpe.ViewModels.Panels.Modding;

public sealed partial class ModDevelopPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleModTemplateInstallCommand))]
    public partial bool DotNetSdkInstalled { get; set; }

    [ObservableProperty]
    public partial bool ModTemplateInstalled { get; set; }

    [ObservableProperty]
    public partial bool EnvVariableSet { get; set; }

    public override async Task InitializeAsync()
    {
        DotNetSdkInstalled = await LocalService.CheckDotNetSdkInstalledAsync().ConfigureAwait(true);
        ModTemplateInstalled = await LocalService.CheckModTemplateInstalledAsync().ConfigureAwait(true);
        EnvVariableSet = PlatformService.CheckPathEnvironmentVariableSet();

        Logger.ZLogInformation($"{nameof(ModDevelopPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallDotNetSdkAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_DotNetSDK_Install).ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Installing DotNet SDK...");
        var success = await PlatformService.InstallDotNetSdkAsync().ConfigureAwait(true);
        if (!success)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_Error_DotNetSDK_Install_Failed).ConfigureAwait(false);
            return;
        }

        Logger.ZLogInformation($"DotNet SDK installed successfully");
        DotNetSdkInstalled = true;
    }

    [RelayCommand(CanExecute = nameof(DotNetSdkInstalled))]
    private Task ToggleModTemplateInstallAsync() =>
        !ModTemplateInstalled ? InstallModTemplateAsync() : UninstallModTemplateAsync();

    private async Task InstallModTemplateAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync("Are you sure you want to install the Mod Template?").ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Installing Mod Template...");
        try
        {
            await PlatformService.InstallModTemplateAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Mod Template installed successfully");
            ModTemplateInstalled = true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install Mod Template");
            await MessageBoxService.ErrorAsync("Failed to install Mod Template").ConfigureAwait(false);
        }
    }

    private async Task UninstallModTemplateAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync("Are you sure you want to uninstall the Mod Template?").ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Uninstalling Mod Template...");
        try
        {
            await PlatformService.UninstallModTemplateAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Mod Template uninstalled successfully");
            ModTemplateInstalled = false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to uninstall Mod Template");
            await MessageBoxService.ErrorAsync("Failed to uninstall Mod Template").ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task SetEnvVariableAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync("Are you sure you want to set MD_DIRECTORY environment variable?").ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Setting MD_DIRECTORY environment variable...");

        var success = PlatformService.SetPathEnvironmentVariable();
        if (success)
        {
            Logger.ZLogInformation($"MD_DIRECTORY environment variable set successfully");
            EnvVariableSet = true;
        }
        else
        {
            Logger.ZLogError($"Failed to set MD_DIRECTORY environment variable");
            await MessageBoxService.ErrorAsync("Failed to set MD_DIRECTORY environment variable").ConfigureAwait(false);
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