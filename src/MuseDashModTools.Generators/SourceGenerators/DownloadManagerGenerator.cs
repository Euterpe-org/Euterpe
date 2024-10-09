using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class DownloadManagerGenerator : IIncrementalGenerator
{
    private static readonly ImmutableArray<string> IgnoredMethodNames =
    [
        "CheckForUpdatesAsync",
        "FetchReadmeAsync"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider.CreateSyntaxProvider(
                FilterNode, ExtractDataFromContext),
            GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { Identifier.Text: "DownloadManager" };

    private static DownloadServiceMethodData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                Node: ClassDeclarationSyntax classDeclarationSyntax,
                SemanticModel: var semanticModel
            })
        {
            return null;
        }

        var symbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
        var interfaceSymbol = symbol.AllInterfaces.Single(x => x.Name == "IDownloadService");
        var methods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>();

        var methodDeclarationDict = new Dictionary<string, MethodData>();

        foreach (var method in methods)
        {
            var methodName = method.Name;
            var parameters = string.Join(", ", method.Parameters.Select(x => x.ToString()));
            var parameterNames = string.Join(", ", method.Parameters.Select(x => x.Name));
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

        foreach (var ignoredMethodName in IgnoredMethodNames)
        {
            methodDeclarationDict.Remove(ignoredMethodName);
        }

        var sb = new StringBuilder();
        foreach (var value in methodDeclarationDict.Values)
        {
            sb.AppendLine($"{GetGeneratedCodeAttribute(nameof(DownloadManagerGenerator))}");
            sb.AppendLine($"public {value.ReturnType} {value.MethodName}({value.MethodParameters})");
            sb.AppendLine($$"""
                            {
                                return Setting.DownloadSource switch
                                {
                                    DownloadSource.GitHub => GitHubDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    DownloadSource.GitHubMirror => GitHubMirrorDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    DownloadSource.Custom => CustomDownloadService.{{value.MethodName}}({{value.MethodParameterNames}}),
                                    _ => throw new UnreachableException()
                                };
                            }
                            """);
        }

        spc.AddSource("DownloadManager.g.cs",
            Header +
            $$"""
              namespace MuseDashModTools.Services;

              partial class DownloadManager
              {
                  {{sb.ToString().TrimEnd()}}
              }
              """);
    }

    private sealed record DownloadServiceMethodData(Dictionary<string, MethodData> Methods);

    private sealed record MethodData(string MethodName, string MethodParameters, string MethodParameterNames, string ReturnType);
}