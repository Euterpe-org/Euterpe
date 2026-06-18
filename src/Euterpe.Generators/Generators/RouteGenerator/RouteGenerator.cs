namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class RouteGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";
    private const string AppSingletonAttributeName = "Euterpe.Shared.Attributes.AppSingletonAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var routes = context.SyntaxProvider
            .ForAttributeWithMetadataName(RouteAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractRoute)
            .Collect();

        context.RegisterSourceOutput(routes, Generate);
    }

    private static RouteData? ExtractRoute(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes:
                [
                    { ConstructorArguments: [{ Value: string path }] } attribute
                ]
            })
        {
            return null;
        }

        var displayName = string.Empty;
        var icon = string.Empty;
        var order = 0;

        foreach (var (key, value) in attribute.NamedArguments)
        {
            switch (key)
            {
                case "DisplayName" when value.Value is string s:
                    displayName = s;
                    break;

                case "Icon" when value.Value is string s:
                    icon = s;
                    break;

                case "Order" when value.Value is int i:
                    order = i;
                    break;
            }
        }

        var isAppSingleton = symbol.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() is AppSingletonAttributeName);

        return new RouteData(symbol.Name, symbol.ContainingNamespace.ToDisplayString(), path, displayName, icon, order, isAppSingleton);
    }

    private static void Generate(SourceProductionContext spc, ImmutableArray<RouteData?> routes)
    {
        if (routes.IsEmpty)
        {
            return;
        }

        var tree = RouteTree.Build(routes);
        spc.AddSource("RouteTree.g.cs", GenerateRouteTree(tree));

        foreach (var (parentPath, children) in tree.ChildrenByParent)
        {
            var (ns, name) = tree.GetViewModel(parentPath);
            spc.AddSource($"{name}.Routes.g.cs", GenerateViewModelRoutes(ns, name, children, parentPath is "/"));
        }
    }

    private sealed record RouteData(string ClassName, string Namespace, string Path, string DisplayName, string Icon, int Order, bool IsAppSingleton);
}
