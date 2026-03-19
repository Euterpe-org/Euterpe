namespace Euterpe.Services;

public sealed class NavigationService
{
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
        var chain = Match(RouteTree.Root, route);
        if (chain is null)
        {
            Logger.ZLogWarning($"Route not found: {route}");
            return;
        }

        foreach (var node in chain)
        {
            node.Select?.Invoke();
        }

        Logger.ZLogInformation($"Navigated to: {route}");
    }

    private static List<RouteNode>? Match(RouteNode node, string route)
    {
        foreach (var child in node.Children)
        {
            if (child.Path == route)
            {
                return [child];
            }

            if (!route.StartsWith(child.Path + "/", StringComparison.Ordinal))
            {
                continue;
            }

            var rest = Match(child, route);
            if (rest is null)
            {
                continue;
            }

            rest.Insert(0, child);
            return rest;
        }

        return null;
    }
}