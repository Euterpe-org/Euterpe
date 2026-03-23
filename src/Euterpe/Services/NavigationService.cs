namespace Euterpe.Services;

public sealed class NavigationService
{
    public AsyncGate Ready { get; } = new();

    public string? CurrentRoute { get; private set; }

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
        if (string.Equals(CurrentRoute, route, StringComparison.Ordinal))
        {
            Logger.ZLogDebug($"Already at route: {route}, skipping navigation");
            return;
        }

        var node = RouteTree.Root;

        while (node.Children.FirstOrDefault(x => route.StartsWith(x.Path, StringComparison.Ordinal)) is { } child)
        {
            child.Select?.Invoke();
            node = child;
        }

        CurrentRoute = route;
        Logger.ZLogInformation($"Navigated to: {route}");
    }

    public async Task NavigateToAsync(string route)
    {
        await Ready.WaitAsync().ConfigureAwait(true);
        NavigateTo(route);
    }
}