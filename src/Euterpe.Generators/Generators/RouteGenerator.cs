namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class RouteGenerator : IncrementalGeneratorBase
{
    protected override string ExpectedRootNamespace => EuterpeNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        // Collect route-annotated views
        var routeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(FilterRouteNode, ExtractRouteData)
            .Where(static x => x is not null);

        // Collect NavViewModelBase subclasses to get their namespaces
        var viewModelProvider = context.SyntaxProvider
            .CreateSyntaxProvider(FilterViewModelNode, ExtractViewModelData)
            .Where(static x => x is not null);

        var combined = routeProvider.Collect()
            .Combine(viewModelProvider.Collect());

        context.RegisterSourceOutput(combined.WithCondition(isValidProvider), GenerateFromData);
    }

    #region Filters

    private static bool FilterRouteNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static bool FilterViewModelNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types }
        && types.Any(x => x.Type.ToString() is "NavViewModelBase");

    #endregion

    #region Extract

    private static RouteData? ExtractRouteData(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            return null;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, ct);
        if (symbol is null)
            return null;

        var routeAttribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "RouteAttribute");

        if (routeAttribute is null)
            return null;

        var path = routeAttribute.ConstructorArguments[0].Value as string;
        if (path is null)
            return null;

        var displayName = "";
        var icon = "";
        var order = 0;

        foreach (var named in routeAttribute.NamedArguments)
        {
            switch (named.Key)
            {
                case "DisplayName":
                    displayName = named.Value.Value as string ?? "";
                    break;
                case "Icon":
                    icon = named.Value.Value as string ?? "";
                    break;
                case "Order":
                    order = (int)(named.Value.Value ?? 0);
                    break;
            }
        }

        var viewNamespace = symbol.ContainingNamespace.ToDisplayString();
        return new RouteData(symbol.Name, viewNamespace, path, displayName, icon, order);
    }

    private static ViewModelInfo? ExtractViewModelData(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            return null;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, ct);
        if (symbol is null)
            return null;

        return new ViewModelInfo(symbol.Name, symbol.ContainingNamespace.ToDisplayString());
    }

    #endregion

    #region Generate

    private static void GenerateFromData(
        SourceProductionContext spc,
        (ImmutableArray<RouteData?> Routes, ImmutableArray<ViewModelInfo?> ViewModels) data)
    {
        var routes = data.Routes
            .Where(x => x is not null)
            .Cast<RouteData>()
            .ToArray();

        var viewModels = data.ViewModels
            .Where(x => x is not null)
            .Cast<ViewModelInfo>()
            .ToDictionary(x => x.Name, x => x.Namespace);

        if (routes is [])
            return;

        // Build parent-child relationships based on path hierarchy
        var childrenByParent = new Dictionary<string, List<RouteData>>();

        foreach (var route in routes)
        {
            var parentPath = GetParentPath(route.Path, routes);

            if (!childrenByParent.TryGetValue(parentPath, out var list))
            {
                list = [];
                childrenByParent[parentPath] = list;
            }

            list.Add(route);
        }

        // Sort children by order within each parent
        foreach (var list in childrenByParent.Values)
            list.Sort((a, b) => a.Order.CompareTo(b.Order));

        // Determine which ViewModels own which children
        var viewModelGroups = new Dictionary<string, (string ViewModelName, string ViewModelNamespace, List<RouteData> Children)>();

        foreach (var (parentPath, children) in childrenByParent)
        {
            string viewModelName;
            if (parentPath is "/")
            {
                viewModelName = "MainWindowViewModel";
            }
            else
            {
                var parentRoute = routes.FirstOrDefault(r => r.Path == parentPath);
                if (parentRoute is null)
                    continue;
                viewModelName = parentRoute.ClassName + "ViewModel";
            }

            var vmNamespace = viewModels.TryGetValue(viewModelName, out var ns) ? ns : "Euterpe.ViewModels";
            viewModelGroups[parentPath] = (viewModelName, vmNamespace, children);
        }

        // Generate ViewModel partial classes
        foreach (var (_, (viewModelName, vmNamespace, children)) in viewModelGroups)
        {
            GenerateViewModelRoutes(spc, viewModelName, vmNamespace, children);
        }

        // Generate RouteTree
        GenerateRouteTree(spc, routes, childrenByParent, viewModelGroups);
    }

    private static void GenerateViewModelRoutes(
        SourceProductionContext spc,
        string viewModelName,
        string viewModelNamespace,
        List<RouteData> children)
    {
        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine($$"""
                        using System.Collections.Frozen;

                        namespace {{viewModelNamespace}};

                        partial class {{viewModelName}}
                        {
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            [global::JetBrains.Annotations.UsedImplicitly]
                            public required global::Euterpe.Services.NavigationService NavigationService { get; init; }

                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Styles.Models.NavItem> NavItems { get; } =
                            [
                        """);

        sb.IncreaseIndent(2);
        foreach (var child in children)
        {
            var iconPart = string.IsNullOrEmpty(child.Icon) ? "" : $" {{ IconResourceKey = \"{child.Icon}\" }}";
            sb.AppendLine($"""new(global::Euterpe.Localization.XAMLLiteral.{child.DisplayName}, "{child.Path}"){iconPart},""");
        }

        sb.ResetIndent();
        sb.AppendLine("        ];");
        sb.AppendLine();

        // Generate FrozenDictionary for ResolveRoute
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            private static readonly FrozenDictionary<string, global::System.Func<global::Euterpe.Services.NavigationService, global::Avalonia.Controls.Control>> RouteLookup =
                                new global::System.Collections.Generic.Dictionary<string, global::System.Func<global::Euterpe.Services.NavigationService, global::Avalonia.Controls.Control>>
                                {
                        """);

        sb.IncreaseIndent(3);
        foreach (var child in children)
        {
            sb.AppendLine($"""["{child.Path}"] = static ns => ns.NavigateTo<global::{child.ViewNamespace}.{child.ClassName}>(),""");
        }

        sb.ResetIndent();
        sb.AppendLine("""
                                }.ToFrozenDictionary();
                        """);

        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(RouteGenerator))}}
                            protected override global::Avalonia.Controls.Control ResolveRoute(string route)
                                => RouteLookup[route](NavigationService);
                        }
                        """);

        spc.AddSource($"{viewModelName}.Routes.g.cs", sb.ToString());
    }

    private static void GenerateRouteTree(
        SourceProductionContext spc,
        RouteData[] allRoutes,
        Dictionary<string, List<RouteData>> childrenByParent,
        Dictionary<string, (string ViewModelName, string ViewModelNamespace, List<RouteData> Children)> viewModelGroups)
    {
        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine("""
                      namespace Euterpe;

                      public static class RouteTree
                      {
                          public static readonly global::Euterpe.Styles.Models.RouteNode Root = new("/", null, [
                      """);

        if (childrenByParent.TryGetValue("/", out var rootChildren))
        {
            sb.IncreaseIndent(2);
            foreach (var child in rootChildren)
            {
                GenerateRouteNode(sb, child, allRoutes, childrenByParent, viewModelGroups);
            }
            sb.ResetIndent();
        }

        sb.AppendLine("""
                          ]);

                          private static T Resolve<T>() where T : notnull => IocContainer.Resolve<T>();
                      }
                      """);

        spc.AddSource("RouteTree.g.cs", sb.ToString());
    }

    private static void GenerateRouteNode(
        IndentedGeneratorStringBuilder sb,
        RouteData route,
        RouteData[] allRoutes,
        Dictionary<string, List<RouteData>> childrenByParent,
        Dictionary<string, (string ViewModelName, string ViewModelNamespace, List<RouteData> Children)> viewModelGroups)
    {
        var parentPath = GetParentPath(route.Path, allRoutes);

        if (!viewModelGroups.TryGetValue(parentPath, out var group))
            return;

        var indexInParent = group.Children.IndexOf(route);
        var vmFullName = $"global::{group.ViewModelNamespace}.{group.ViewModelName}";

        var selectLambda = $"() => {{ var vm = Resolve<{vmFullName}>(); vm.SelectedItem = vm.NavItems[{indexInParent}]; }}";

        var hasChildren = childrenByParent.ContainsKey(route.Path);

        if (hasChildren)
        {
            sb.AppendLine($"""new("{route.Path}", {selectLambda}, [""");
            sb.IncreaseIndent();

            foreach (var child in childrenByParent[route.Path])
            {
                GenerateRouteNode(sb, child, allRoutes, childrenByParent, viewModelGroups);
            }

            sb.DecreaseIndent();
            sb.AppendLine("]),");
        }
        else
        {
            sb.AppendLine($"""new("{route.Path}", {selectLambda}, []),""");
        }
    }

    #endregion

    #region Helpers

    private static string GetParentPath(string path, RouteData[] allRoutes)
    {
        var bestMatch = "/";
        foreach (var route in allRoutes)
        {
            if (route.Path == path)
                continue;

            if (path.StartsWith(route.Path + "/", StringComparison.Ordinal) && route.Path.Length > bestMatch.Length)
                bestMatch = route.Path;
        }

        return bestMatch;
    }

    #endregion

    #region Records

    private sealed record RouteData(string ClassName, string ViewNamespace, string Path, string DisplayName, string Icon, int Order);
    private sealed record ViewModelInfo(string Name, string Namespace);

    #endregion
}
