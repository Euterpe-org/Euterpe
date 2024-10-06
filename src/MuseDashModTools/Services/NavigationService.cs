namespace MuseDashModTools.Services;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    private Control? _content;

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public void NavigateTo<TView>() where TView : Control, new()
    {
        Logger.Information("Navigating to View: {View}", typeof(TView).Name);
        Content = App.Current.Container.Resolve<TView>();
    }
}