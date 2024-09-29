namespace MuseDashModTools.Contracts;

public interface INavigationService
{
    Control? Content { get; }

    void NavigateTo<TView>() where TView : Control, new();
}