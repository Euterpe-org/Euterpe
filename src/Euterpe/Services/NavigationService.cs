namespace Euterpe.Services;

public sealed class NavigationService
{
    public AsyncGate Ready { get; } = new();

    #region Injections

    [UsedImplicitly]
    public required ILogger<NavigationService> Logger { get; init; }

    #endregion Injections

    public Control NavigateTo<TView>() where TView : Control, new()
    {
        Logger.ZLogInformation($"Navigating to View: {typeof(TView).Name}");
        return IocContainer.Resolve<TView>();
    }

    public void NavigateTo(string route)
    {
        var node = RouteTree.Root;

        while (node.Children.FirstOrDefault(x => route.StartsWith(x.Path)) is { } child)
        {
            child.Select?.Invoke();
            node = child;
        }

        Logger.ZLogInformation($"Navigated to: {route}");
    }

    public async Task NavigateToAsync(string route)
    {
        await Ready.WaitAsync().ConfigureAwait(true);
        NavigateTo(route);
    }
}