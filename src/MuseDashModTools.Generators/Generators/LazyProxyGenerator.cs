namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class LazyProxyGenerator : IncrementalGeneratorBase
{
    private const string LazyProxyAttributeName = "MuseDashModTools.Common.Attributes.LazyProxyAttribute";

    protected override string ExpectedRootNamespace => MuseDashModToolsCoreNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            LazyProxyAttributeName, FilterNode, ExtractDataFromContext);
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
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

        var attribute = attributes.Single(x => x.AttributeClass!.ToDisplayString() == LazyProxyAttributeName);
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

        var typeName = baseType.Name;
        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        namespace {{nameSpace}};

                        partial class {{className}}
                        {
                            public Lazy<{{typeName}}> {{typeName}} { get; init; } = null!;

                        """);
        sb.IncreaseIndent();

        var methods = baseType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m =>
                m.MethodKind == MethodKind.Ordinary &&
                m is { DeclaredAccessibility: Accessibility.Public, IsStatic: false });

        foreach (var method in methods)
        {
            sb.AppendLine(GetGeneratedCodeAttribute(nameof(LazyProxyGenerator)));
            var returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToDisplayString();
            sb.AppendLine(
                $"public {returnType} {method.Name}({string.Join(", ", method.Parameters.Select(x => $"{x.Type} {x.Name}"))}) => {typeName}.Value.{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Name))});");
            sb.AppendLine();
        }

        var properties = baseType.GetMembers().OfType<IPropertySymbol>()
            .Where(x => x is { DeclaredAccessibility: Accessibility.Public, IsStatic: false });
        foreach (var property in properties)
        {
            sb.AppendLine(GetGeneratedCodeAttribute(nameof(LazyProxyGenerator)));
            sb.AppendLine($"public {property.Type} {property.Name} => {typeName}.Value.{property.Name};");
            sb.AppendLine();
        }

        sb.ResetIndent();
        sb.AppendLine("}");
        spc.AddSource($"{className}.Proxy.g.cs", sb.ToString());
    }

    private sealed record ClassData(string NameSpace, string ClassName, INamedTypeSymbol BaseTypeSymbol);
}