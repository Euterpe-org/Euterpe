namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class RouteGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            RouteAttributeName, FilterNode, ExtractDataFromContext).Collect();
        context.RegisterSourceOutput(syntaxProvider, GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    private static RouteData? ExtractDataFromContext(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: var attributes
            })
        {
            return null;
        }

        var attribute = attributes.Single(x => x.AttributeClass!.ToDisplayString() == RouteAttributeName);
        if (attribute.ConstructorArguments[0].Value is not string path)
        {
            return null;
        }

        var isPerGame = symbol.GetAttributes()
            .Any(static a => a.AttributeClass?.ToDisplayString() is "Euterpe.Shared.Attributes.PerGameAttribute");

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

        return new RouteData(symbol.Name, symbol.ContainingNamespace.ToDisplayString(), path, displayName, icon, order, isPerGame);
    }

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<RouteData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var tree = RouteTree.Build(dataCollection);

        spc.AddSource("RouteTree.g.cs", GenerateRouteTree(tree));

        foreach (var (parentPath, children) in tree.ChildrenByParent)
        {
            var (ns, name) = tree.GetViewModel(parentPath);
            spc.AddSource($"{name}.Routes.g.cs", GenerateViewModelRoutes(ns, name, children, parentPath is "/"));
        }
    }

    private static (string Namespace, string Name) DeriveViewModel(RouteData route) => (route.Namespace, route.ClassName);

    private sealed record RouteData(string ClassName, string Namespace, string Path, string DisplayName, string Icon, int Order, bool IsPerGame);
}