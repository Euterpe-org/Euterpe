namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class LazyProxyGenerator : IncrementalGeneratorBase
{
    private const string LazyProxyAttributeName = "MuseDashModTools.Common.Attributes.LazyProxyAttribute";

    protected override string ExpectedRootNamespace => MuseDashModToolsCoreNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            LazyProxyAttributeName, FilterNode, ExtractDataFromContext).Collect();
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

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ClassData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine($$"""
                        namespace MuseDashModTools.Core.Extensions;

                        public static class LazyProxyExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(LazyProxyGenerator))}}
                            public static void RegisterLazyProxies(this ContainerBuilder builder)
                            {
                        """);

        sb.IncreaseIndent(2);
        foreach (var data in dataCollection)
        {
            if (data is not var (nameSpace, className, baseType))
            {
                continue;
            }

            GenerateSingleProxyClass(spc, nameSpace, className, baseType);

            sb.AppendLine($"builder.RegisterType<{nameSpace}.{className}>().PropertiesAutowired().SingleInstance();");
        }

        sb.ResetIndent();
        sb.AppendLine("""
                          }
                      }
                      """);

        spc.AddSource("LazyProxyExtensions.g.cs", sb.ToString());
    }

    private static void GenerateSingleProxyClass(SourceProductionContext spc, string nameSpace, string className, INamedTypeSymbol baseType)
    {
        var sb = new IndentedGeneratorStringBuilder();

        var typeName = baseType.Name;
        sb.AppendLine($$"""
                        namespace {{nameSpace}};

                        partial class {{className}}
                        {
                            public required Lazy<{{typeName}}> {{typeName}} { get; init; }

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