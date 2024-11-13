namespace MuseDashModTools.Services;

public sealed partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    public partial Control? Content { get; private set; }

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public void NavigateTo<TView>() where TView : Control, new()
    {
        Logger.Information("Navigating to View: {View}", typeof(TView).Name);
        Content = App.Container.Resolve<TView>();
    }
}