namespace MuseDashModTools.ViewModels;

public partial class PageViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; protected set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; protected set; }

    protected virtual void Navigate(PageNavItem? value)
    {
    }

    partial void OnSelectedItemChanged(PageNavItem? value)
    {
        Navigate(value);
    }
}