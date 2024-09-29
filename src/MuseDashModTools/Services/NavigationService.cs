namespace MuseDashModTools.Services;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    private readonly Dictionary<string, Control> _viewCache = new();

    [ObservableProperty]
    private Control? _content;

    private string _currentViewName = string.Empty;

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public void NavigateTo<TView>() where TView : Control, new()
    {
        var viewName = typeof(TView).Name;

        if (_currentViewName == viewName)
        {
            Logger.Information("View: {View} is already navigated", viewName);
            return;
        }

        if (_viewCache.TryGetValue(viewName, out var cachedView))
        {
            SetContent(viewName, cachedView);
            Logger.Information("Using cached View: {View}", viewName);
            return;
        }

        Logger.Information("Navigating to View: {View}", viewName);
        if (Activator.CreateInstance<TView>() is Control view)
        {
            SetContent(viewName, view);
            _viewCache[viewName] = view;
            return;
        }

        Logger.Error("View not found for View: {View}", viewName);
    }

    private void SetContent(string viewName, Control view)
    {
        _currentViewName = viewName;
        Content = view;
    }
}