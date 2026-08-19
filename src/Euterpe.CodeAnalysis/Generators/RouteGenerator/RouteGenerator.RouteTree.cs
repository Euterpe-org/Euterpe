using System.Collections.Frozen;

namespace Euterpe.CodeAnalysis;

public sealed partial class RouteGenerator
{
    private sealed class RouteTree
    {
        private readonly FrozenDictionary<string, RouteData> _routeByPath;
        public FrozenDictionary<string, ImmutableArray<RouteData>> ChildrenByParent { get; }

        public ImmutableArray<RouteData> RootChildren =>
            ChildrenByParent.TryGetValue("/", out var children) ? children : [];

        private RouteTree(
            FrozenDictionary<string, RouteData> routeByPath,
            FrozenDictionary<string, ImmutableArray<RouteData>> childrenByParent)
        {
            _routeByPath = routeByPath;
            ChildrenByParent = childrenByParent;
        }

        public (string Namespace, string Name) GetViewModel(string routePath)
        {
            var route = _routeByPath[routePath];
            return (route.Namespace, route.ClassName);
        }

        public static RouteTree Build(ImmutableArray<RouteData?> routes)
        {
            var routeByPath = new Dictionary<string, RouteData>(routes.Length);
            var childrenBuilder = new Dictionary<string, List<RouteData>>();

            foreach (var route in routes)
            {
                if (route is not { Path: var path })
                {
                    continue;
                }

                routeByPath[path] = route;

                if (path is "/")
                {
                    continue;
                }

                var lastSlash = path.LastIndexOf('/');
                var parentPath = lastSlash > 0 ? path[..lastSlash] : "/";

                if (!childrenBuilder.TryGetValue(parentPath, out var children))
                {
                    children = [];
                    childrenBuilder[parentPath] = children;
                }

                children.Add(route);

                // After Source Generator can use .NET 10
                /*ref var children = ref CollectionsMarshal.GetValueRefOrAddDefault(childrenBuilder, parentPath, out _);
                children ??= [];
                children.Add(data);*/
            }

            var frozenChildren = new Dictionary<string, ImmutableArray<RouteData>>(childrenBuilder.Count);

            foreach (var (parentPath, children) in childrenBuilder)
            {
                children.Sort(static (a, b) => a.Order.CompareTo(b.Order));
                frozenChildren[parentPath] = [.. children];
            }

            return new RouteTree(routeByPath.ToFrozenDictionary(), frozenChildren.ToFrozenDictionary());
        }
    }
}
