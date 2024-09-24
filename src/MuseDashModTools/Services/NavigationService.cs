namespace MuseDashModTools.Services;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    private Control? _content;

    private string _currentViewName = string.Empty;

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public void NavigateToViewModel<TViewModel>() where TViewModel : ViewModelBase, new()
    {
        var viewModelType = typeof(TViewModel);
        var viewType = _viewModelToViewMap[viewModelType];

        if (_currentViewName == viewType.Name)
        {
            Logger.Information("ViewModel: {ViewModel} is already navigated", viewModelType.Name);
            return;
        }

        Logger.Information("Navigating to ViewModel: {ViewModel}", viewModelType.Name);
        _currentViewName = viewType.Name;

        if (Activator.CreateInstance(viewType) is Control view)
        {
            Content = view;
            return;
        }

        Logger.Error("View not found for ViewModel: {ViewModel}", viewModelType.Name);
    }

    public void NavigateToView<TView>() where TView : Control, new()
    {
        var viewType = typeof(TView);

        if (_currentViewName == viewType.Name)
        {
            Logger.Information("View: {View} is already navigated", viewType.Name);
            return;
        }

        Logger.Information("Navigating to View: {View}", viewType.Name);
        _currentViewName = viewType.Name;

        if (Activator.CreateInstance<TView>() is Control view)
        {
            Content = view;
            return;
        }

        Logger.Error("View not found for View: {View}", viewType.Name);
    }
}