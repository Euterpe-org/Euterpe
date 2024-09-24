namespace MuseDashModTools.Contracts;

public interface INavigationService
{
    Control? Content { get; }
    void NavigateToViewModel<TViewModel>() where TViewModel : ViewModelBase, new();

    void NavigateToView<TView>() where TView : Control, new();
}