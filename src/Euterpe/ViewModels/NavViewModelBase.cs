namespace Euterpe.ViewModels;

public abstract partial class NavViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedItem { get; set; }

    public abstract IReadOnlyList<NavItem> NavItems { get; }

    protected abstract Control ResolveRoute(string route);

    partial void OnSelectedItemChanged(NavItem? value)
    {
        if (value is not null)
        {
            Content = ResolveRoute(value.NavigateKey);
        }
    }

    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SelectedItem = NavItems[0];
        return Task.CompletedTask;
    }
}