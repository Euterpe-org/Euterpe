namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class DownloadManagerGenerator : IncrementalGeneratorBase
{
    private const string DownloadManagerAttributeName = "MuseDashModTools.Common.Attributes.DownloadManagerAttribute";
    protected override string ExpectedRootNamespace => MuseDashModToolsCoreNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            DownloadManagerAttributeName, FilterNode, ExtractDataFromContext);
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax;

    private static DownloadServiceMethodData? ExtractDataFromContext(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                TargetNode: ClassDeclarationSyntax classDeclarationSyntax,
                SemanticModel: var semanticModel
            })
        {
            return null;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
        var interfaceSymbol = classSymbol.AllInterfaces.Single(x => x.Name is "IDownloadService");

        var methodsToImplement = interfaceSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => classSymbol.FindImplementationForInterfaceMember(method) is not { DeclaringSyntaxReferences.IsEmpty: false })
            .Select(method => new MethodData(
                method.Name,
                string.Join(", ", method.Parameters.Select(p => p.ToDisplayString())),
                string.Join(", ", method.Parameters.Select(p => p.Name)),
                method.ReturnType.ToDisplayString()
            ))
            .ToDictionary(m => m.MethodName, m => m);

        return new DownloadServiceMethodData(methodsToImplement);
    }

    private static void GenerateFromData(SourceProductionContext spc, DownloadServiceMethodData? data)
    {
        if (data is not var (methodDeclarationDict))
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
        foreach (var value in methodDeclarationDict.Values)
        {
            sb.AppendLine($"{GetGeneratedCodeAttribute(nameof(DownloadManagerGenerator))}");
            sb.AppendLine($"public {value.ReturnType} {value.MethodName}({value.MethodParameters})");
            sb.AppendLine($$"""
                            {
                                return Config.DownloadSource switch
                                {
                                    DownloadSource.GitHub => GitHubDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    DownloadSource.GitHubMirror => GitHubMirrorDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    DownloadSource.Gitee => GiteeDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    DownloadSource.Custom => CustomDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    _ => throw new UnreachableException()
                                };
                            }
                            """);
        }

        sb.ResetIndent();
        sb.AppendLine("}");

        spc.AddSource("DownloadManager.g.cs", sb.ToString());
    }

    private sealed record DownloadServiceMethodData(Dictionary<string, MethodData> Methods);

    private sealed record MethodData(string MethodName, string MethodParameters, string MethodParameterNames, string ReturnType);
}