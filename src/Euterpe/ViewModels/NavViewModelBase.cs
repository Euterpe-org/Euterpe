namespace Euterpe.ViewModels;

public abstract partial class NavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control Content { get; set; } = null!;

    [ObservableProperty]
    public partial NavItem SelectedItem { get; set; } = null!;

    public abstract IReadOnlyList<NavItem> NavItems { get; }

    protected abstract Control ResolveRoute(string route);

    partial void OnSelectedItemChanged(NavItem value) => Content = ResolveRoute(value.NavigateKey);

    protected override Task OnInitializeAsync()
    {
        SelectedItem = NavItems[0];
        return base.OnInitializeAsync();
    }
}