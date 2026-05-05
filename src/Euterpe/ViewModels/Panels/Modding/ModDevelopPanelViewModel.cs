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

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        DotNetSdkInstalled = await SdkInstaller.CheckInstalledAsync().ConfigureAwait(true);
        ModTemplateInstalled = await ModTemplateInstaller.CheckInstalledAsync().ConfigureAwait(true);
        EnvVariableSet = PathEnvironment.IsSet();

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
        var success = await SdkInstaller.InstallAsync().ConfigureAwait(true);
        if (!success)
        {
            await MessageBoxService.ErrorAsync(MessageBox_Content_DotNetSDK_Install_Failed).ConfigureAwait(false);
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
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ModTemplate_Install).ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Installing Mod Template...");
        try
        {
            await ModTemplateInstaller.InstallAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Mod Template installed successfully");
            ModTemplateInstalled = true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install Mod Template");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ModTemplate_Install_Failed).ConfigureAwait(false);
        }
    }

    private async Task UninstallModTemplateAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_ModTemplate_Uninstall).ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Uninstalling Mod Template...");
        try
        {
            await ModTemplateInstaller.UninstallAsync().ConfigureAwait(true);
            Logger.ZLogInformation($"Mod Template uninstalled successfully");
            ModTemplateInstalled = false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to uninstall Mod Template");
            await MessageBoxService.ErrorAsync(MessageBox_Content_ModTemplate_Uninstall_Failed).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task SetEnvVariableAsync()
    {
        var result = await MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_SetMdDirectoryEnvironment)
            .ConfigureAwait(true);
        if (result is not MessageBoxResult.Yes)
        {
            return;
        }

        Logger.ZLogInformation($"Setting {GameConfig.PathEnvironmentVariableName} environment variable...");

        var success = PathEnvironment.Set();
        if (success)
        {
            Logger.ZLogInformation($"{GameConfig.PathEnvironmentVariableName} environment variable set successfully");
            EnvVariableSet = true;
        }
        else
        {
            Logger.ZLogError($"Failed to set {GameConfig.PathEnvironmentVariableName} environment variable");
            await MessageBoxService.ErrorAsync(MessageBox_Content_SetMdDirectoryEnvironment_Failed).ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<ModDevelopPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IDotNetSdkInstaller SdkInstaller { get; init; }

    [UsedImplicitly]
    public required IGameModTemplateInstaller ModTemplateInstaller { get; init; }

    [UsedImplicitly]
    public required IGamePathEnvironment PathEnvironment { get; init; }

    #endregion Injections
}