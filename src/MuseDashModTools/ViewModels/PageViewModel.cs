namespace MuseDashModTools.ViewModels;

public partial class PageViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; set; }

    public abstract void OnSelectedItemChanged();

    partial void OnSelectedItemChanged(PageNavItem? value)
    { }
}