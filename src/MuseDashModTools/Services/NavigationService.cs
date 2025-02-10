namespace MuseDashModTools.Services;

public sealed partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    public partial Control? PageContent { get; private set; }

    [ObservableProperty]
    public partial Control? PanelModdingContent { get; private set; }

    [ObservableProperty]
    public partial Control? PanelChartingContent { get; private set; }

    [ObservableProperty]
    public partial Control? PanelSettingContent { get; private set; }

    #region Injections

    [UsedImplicitly]
    public ILogger<NavigationService> Logger { get; init; } = null!;

    #endregion Injections

    public void NavigateToPage<TView>() where TView : Control, new()
    {
        Logger.ZLogInformation($"Navigating to Page View: {typeof(TView).Name}");
        PageContent = App.Container.Resolve<TView>();
    }

    public void NavigateToPanel<TView>(string token) where TView : Control, new()
    {
        Logger.ZLogInformation($"Navigating to Panel View: {typeof(TView).Name}");
        switch (token)
        {
            case "NavigatePanelModding":
                PanelModdingContent = App.Container.Resolve<TView>();
                break;
            case "NavigatePanelCharting":
                PanelChartingContent = App.Container.Resolve<TView>();
                break;
            case "NavigatePanelSetting":
                PanelSettingContent = App.Container.Resolve<TView>();
                break;
        }
    }
}