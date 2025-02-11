namespace MuseDashModTools.Services;

public sealed class NavigationService : ObservableObject
{
    #region Injections

    [UsedImplicitly]
    public ILogger<NavigationService> Logger { get; init; } = null!;

    #endregion Injections

    public Control NavigateTo<TView>() where TView : Control, new()
    {
        Logger.ZLogInformation($"Navigating to View: {typeof(TView).Name}");
        return App.Container.Resolve<TView>();
    }
}