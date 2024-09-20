namespace MuseDashModToolsUI.Services;

public sealed partial class NavigationService : INavigationService
{
    private Control _currentView = null!;
    private string _currentViewName = string.Empty;

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public Control? NavigateToViewModel<TViewModel>() where TViewModel : ViewModelBase, new()
    {
        var viewModelType = typeof(TViewModel);
        var viewType = _viewModelToViewMap[viewModelType];

        if (_currentViewName == viewType.Name)
        {
            Logger.Information("ViewModel: {ViewModel} is already navigated", viewModelType.Name);
            return _currentView;
        }

        Logger.Information("Navigating to ViewModel: {ViewModel}", viewModelType.Name);
        _currentViewName = viewType.Name;

        if (Activator.CreateInstance(viewType) is Control view)
        {
            _currentView = view;
            return view;
        }

        Logger.Error("View not found for ViewModel: {ViewModel}", viewModelType.Name);
        return null;
    }

    public Control? NavigateToView<TView>() where TView : Control, new()
    {
        var viewType = typeof(TView);

        if (_currentViewName == viewType.Name)
        {
            Logger.Information("View: {View} is already navigated", viewType.Name);
            return _currentView;
        }

        Logger.Information("Navigating to View: {View}", viewType.Name);
        _currentViewName = viewType.Name;

        if (Activator.CreateInstance<TView>() is Control view)
        {
            _currentView = view;
            return view;
        }

        Logger.Error("View not found for View: {View}", viewType.Name);
        return null;
    }
}