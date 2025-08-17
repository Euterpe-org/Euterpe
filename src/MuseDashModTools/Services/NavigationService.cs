namespace MuseDashModTools.Services;

public sealed class NavigationService
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<NavigationService> Logger { get; init; }

    #endregion Injections

    public Control NavigateTo<TView>() where TView : Control, new()
    {
        Logger.ZLogInformation($"Navigating to View: {typeof(TView).Name}");
        return IocContainer.Resolve<TView>();
    }
}