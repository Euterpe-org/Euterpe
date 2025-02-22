using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveUI;

namespace MuseDashModTools.ViewModels;

public partial class NavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    [UsedImplicitly]
    public virtual ObservableCollection<NavItem> NavItems { get; } = null!;

    public NavViewModelBase()
    {
        this.WhenActivated((CompositeDisposable _) => Initialize());
    }

    protected virtual void Navigate(NavItem? value)
    {
    }

    protected override void Initialize()
    {
        SelectedItem = NavItems[0];
    }

    [UsedImplicitly]
    partial void OnSelectedItemChanged(NavItem? value)
    {
        Navigate(value);
    }
}