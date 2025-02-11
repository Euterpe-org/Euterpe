using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels;

public partial class PageViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    [UsedImplicitly]
    public virtual ObservableCollection<NavItem> NavItems { get; } = null!;

    protected virtual void Navigate(NavItem? value)
    {
    }

    protected virtual void Initialize()
    {
        SelectedItem = NavItems[0];
    }

    [UsedImplicitly]
    partial void OnSelectedItemChanged(NavItem? value)
    {
        Navigate(value);
    }
}