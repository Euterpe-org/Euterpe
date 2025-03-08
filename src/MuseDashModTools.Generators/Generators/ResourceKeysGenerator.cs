namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ResourceKeysGenerator : IIncrementalGenerator
{
    private const string ResourceKeysAttributeName = "MuseDashModTools.Common.Attributes.ResourceKeysAttribute";

    private static readonly string[] Exceptions =
    [
        "ResourceManager",
        "Culture"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider.ForAttributeWithMetadataName(
                ResourceKeysAttributeName, FilterNode, ExtractDataFromContext),
            GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    private static ClassData? ExtractDataFromContext(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes: var attributes
            })
        {
            return null;
        }

        var attribute = attributes.Single(x => x.AttributeClass!.ToDisplayString() == ResourceKeysAttributeName);
        return attribute.ConstructorArguments[0].Value is not INamedTypeSymbol baseType
            ? null
            : new ClassData(symbol.ContainingNamespace.ToDisplayString(), symbol.Name, baseType);
    }

    private static void GenerateFromData(SourceProductionContext spc, ClassData? data)
    {
        if (data is not var (nameSpace, className, baseType))
        {
            return;
        }

        var sb = new IndentedStringBuilder();

        var propertyNames = baseType.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(x => x.Name)
            .Except(Exceptions);

        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        namespace {{nameSpace}};

                        partial class {{className}}
                        {
                        """);
        sb.IncreaseIndent();

        foreach (var propertyName in propertyNames)
        {
            sb.AppendLine($"public static string {propertyName} => nameof({propertyName});");
        }

        sb.ResetIndent();
        sb.AppendLine("}");
        spc.AddSource($"{className}.ResourceKeys.g.cs", sb.ToString());
    }

    private sealed record ClassData(string NameSpace, string ClassName, INamedTypeSymbol BaseTypeSymbol);
}