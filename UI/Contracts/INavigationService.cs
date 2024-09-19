namespace MuseDashModToolsUI.Contracts;

public interface INavigationService
{
    Control? NavigateToViewModel<TViewModel>() where TViewModel : ViewModelBase, new();

    Control? NavigateToView<TView>() where TView : Control, new();
}