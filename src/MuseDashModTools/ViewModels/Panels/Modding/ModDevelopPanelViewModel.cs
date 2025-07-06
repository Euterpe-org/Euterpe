namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModDevelopPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool DotNetSdkInstalled { get; set; }

    [ObservableProperty]
    public partial bool ModTemplateInstalled { get; set; }

    public override async Task InitializeAsync()
    {
        DotNetSdkInstalled = await LocalService.CheckDotNetSdkInstalledAsync().ConfigureAwait(false);
        ModTemplateInstalled = await LocalService.CheckModTemplateInstalledAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(ModDevelopPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallDotNetSdkAsync()
    {
        var result = await PlatformService.InstallDotNetSdkAsync().ConfigureAwait(false);
        if (!result)
        {
            await MessageBoxService.ErrorAsync("Failed to install DotNet SDK").ConfigureAwait(false);
        }

        DotNetSdkInstalled = true;
    }

    [RelayCommand(CanExecute = nameof(DotNetSdkInstalled))]
    private async Task InstallModTemplateAsync()
    {
        try
        {
            await PlatformService.InstallModTemplateAsync().ConfigureAwait(false);
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