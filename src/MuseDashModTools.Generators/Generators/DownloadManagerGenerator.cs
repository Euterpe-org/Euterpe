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

        var symbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
        var interfaceSymbol = symbol.AllInterfaces.Single(x => x.Name is "IDownloadService");
        var methods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>();

        var methodDeclarationDict = new Dictionary<string, MethodData>();

        foreach (var method in methods)
        {
            var implementingMember = symbol.FindImplementationForInterfaceMember(method);

            var isUserImplemented = implementingMember is { DeclaringSyntaxReferences.IsEmpty: false };

            if (isUserImplemented)
            {
                continue;
            }

            var methodName = method.Name;
            var parameters = string.Join(", ", method.Parameters.Select(p => p.ToString()));
            var parameterNames = string.Join(", ", method.Parameters.Select(p => p.Name));
            var returnType = method.ReturnType.ToDisplayString();
            methodDeclarationDict.Add(methodName, new MethodData(methodName, parameters, parameterNames, returnType));
        }


        return new DownloadServiceMethodData(methodDeclarationDict);
    }

    private static void GenerateFromData(SourceProductionContext spc, DownloadServiceMethodData? data)
    {
        if (data is not var (methodDeclarationDict))
        {
            return;
        }

        var sb = new StringBuilder();
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

        spc.AddSource("DownloadManager.g.cs",
            Header +
            $$"""
              namespace MuseDashModTools.Core;

              partial class DownloadManager
              {
                  {{sb.ToString().TrimEnd()}}
              }
              """);
    }

    private sealed record DownloadServiceMethodData(Dictionary<string, MethodData> Methods);

    private sealed record MethodData(string MethodName, string MethodParameters, string MethodParameterNames, string ReturnType);
}