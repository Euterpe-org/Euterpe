namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class LazyProxyExtensionsGenerator : IncrementalGeneratorBase
{
    private const string LazyProxyAttributeName = "MuseDashModTools.Common.Attributes.LazyProxyAttribute";

    protected override string ExpectedRootNamespace => MuseDashModToolsCoreNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            LazyProxyAttributeName, FilterNode, ExtractDataFromContext).Collect();
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    private static ClassData? ExtractDataFromContext(GeneratorAttributeSyntaxContext context, CancellationToken _) =>
        context is not { TargetSymbol: INamedTypeSymbol symbol }
            ? null
            : new ClassData(symbol.ContainingNamespace.ToDisplayString(), symbol.Name);

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ClassData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new IndentedStringBuilder();

        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        namespace MuseDashModTools.Core.Extensions;

                        public static class LazyProxyExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(LazyProxyExtensionsGenerator))}}
                            public static void RegisterLazyProxies(this ContainerBuilder builder)
                            {
                        """);

        sb.IncreaseIndent(2);
        foreach (var data in dataCollection)
        {
            if (data is not var (nameSpace, className))
            {
                continue;
            }

            sb.AppendLine($"builder.RegisterType<{nameSpace}.{className}>().PropertiesAutowired().SingleInstance();");
        }

        sb.ResetIndent();
        sb.AppendLine("""
                          }
                      }
                      """);

        spc.AddSource("LazyProxyExtensions.g.cs", sb.ToString());
    }

    private sealed record ClassData(string NameSpace, string ClassName);
}