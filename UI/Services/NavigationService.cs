using MuseDashModToolsUI.Views.Pages;

namespace MuseDashModToolsUI.Services;

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<Type, Type> _viewModelToViewMap = new()
    {
        { typeof(ModManagePageViewModel), typeof(ModManagePage) },
        { typeof(SettingPageViewModel), typeof(SettingPage) }
    };

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections

    public Control? NavigateToViewModel<TViewModel>() where TViewModel : ViewModelBase, new()
    {
        var viewModelType = typeof(TViewModel);
        var viewType = _viewModelToViewMap[viewModelType];

        Logger.Information("Navigating to ViewModel: {ViewModel}", viewModelType.Name);

        if (Activator.CreateInstance(viewType) is Control view)
        {
            return view;
        }

        Logger.Error("View not found for ViewModel: {ViewModel}", viewModelType.Name);
        return null;
    }

    public Control? NavigateToView<TView>() where TView : Control, new()
    {
        Logger.Information("Navigating to View: {View}", typeof(TView).Name);

        if (Activator.CreateInstance<TView>() is Control view)
        {
            return view;
        }

        Logger.Error("View not found for View: {View}", typeof(TView).Name);
        return null;
    }
}