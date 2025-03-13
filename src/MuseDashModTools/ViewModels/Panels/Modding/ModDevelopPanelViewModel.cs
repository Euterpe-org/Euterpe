namespace MuseDashModTools.ViewModels.Panels.Modding;

public sealed partial class ModDevelopPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial bool DotNetSdkInstalled { get; set; }

    [ObservableProperty]
    public partial bool ModTemplateInstalled { get; set; }

    public override async Task InitializeAsync()
    {
        // TODO

        // For test
        DotNetSdkInstalled = true;
        ModTemplateInstalled = false;

        Logger.ZLogInformation($"{nameof(ModDevelopPanelViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task InstallDotNetSdk()
    {
        // TODO

        DotNetSdkInstalled = true;
    }

    [RelayCommand]
    private async Task InstallModTemplate()
    {
        // TODO

        ModTemplateInstalled = true;
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<ModDevelopPanelViewModel> Logger { get; init; }

    #endregion Injections
}