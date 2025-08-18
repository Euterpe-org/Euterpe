namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class DownloadManagerGenerator : IncrementalGeneratorBase
{
    private const string DownloadManagerAttributeName = "MuseDashModTools.Common.Attributes.DownloadManagerAttribute";

    private static readonly SymbolDisplayFormat _formatWithDefaultValue = new(
        parameterOptions:
        SymbolDisplayParameterOptions.IncludeType |
        SymbolDisplayParameterOptions.IncludeName |
        SymbolDisplayParameterOptions.IncludeDefaultValue
    );

    protected override string ExpectedRootNamespace => MuseDashModToolsCoreNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            DownloadManagerAttributeName, FilterNode, ExtractDataFromContext);
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    private static MethodData[]? ExtractDataFromContext(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context is not
            {
                TargetNode: ClassDeclarationSyntax classDeclarationSyntax,
                SemanticModel: var semanticModel
            })
        {
            return null;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax, ct)!;
        var interfaceSymbol = classSymbol.AllInterfaces.Single(x => x.Name is "IDownloadService");

        var methodsToImplement = interfaceSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => classSymbol.FindImplementationForInterfaceMember(method) is not { DeclaringSyntaxReferences.IsEmpty: false })
            .Select(method => new MethodData(
                method.Name,
                string.Join(", ", method.Parameters.Select(p => p.ToDisplayString(_formatWithDefaultValue))),
                string.Join(", ", method.Parameters.Select(p => p.Name)),
                method.ReturnType.ToDisplayString()
            ))
            .ToArray();

        return methodsToImplement;
    }

    private static void GenerateFromData(SourceProductionContext spc, MethodData[]? dataCollection)
    {
        if (dataCollection is null or [])
        {
            return;
        }

        var sb = new IndentedGeneratorStringBuilder();
        sb.AppendLine("""
                      namespace MuseDashModTools.Core;

                      partial class DownloadManager
                      {
                      """);

        sb.IncreaseIndent();
        foreach (var method in dataCollection)
        {
            sb.AppendLine($"{GetGeneratedCodeAttribute(nameof(DownloadManagerGenerator))}");
            sb.AppendLine($"public {method.ReturnType} {method.MethodName}({method.MethodParameters}) =>");
            sb.AppendLine($$"""
                                Config.DownloadSource switch
                                {
                                    DownloadSource.GitHub => GitHubDownloadService.{{method.MethodName}}({{method.MethodParameterNames}}),
                                    DownloadSource.GitHubMirror => GitHubMirrorDownloadService.{{method.MethodName}}({{method.MethodParameterNames}}),
                                    DownloadSource.Gitee => GiteeDownloadService.{{method.MethodName}}({{method.MethodParameterNames}}),
                                    DownloadSource.Custom => CustomDownloadService.{{method.MethodName}}({{method.MethodParameterNames}}),
                                    _ => throw new UnreachableException()
                                };
                            """);
            sb.AppendLine();
        }

        sb.ResetIndent();
        sb.AppendLine("}");

        spc.AddSource("DownloadManager.g.cs", sb.ToString());
    }

    private sealed record MethodData(string MethodName, string MethodParameters, string MethodParameterNames, string ReturnType);
}