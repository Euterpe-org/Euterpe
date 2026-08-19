namespace Euterpe.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public sealed class EuterpeApiUrlGenerator : IIncrementalGenerator
{
    private const string BasePathFieldName = "BasePath";
    private const string PathSuffix = "Path";
    private const string UrlSuffix = "Url";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var apiTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { Identifier.ValueText: "EuterpeApi" },
                ExtractApi);

        context.RegisterSourceOutput(apiTypes, Generate);
    }

    private static ApiData? ExtractApi(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol apiType)
        {
            return null;
        }

        var groups = new List<ApiGroupData>();

        foreach (var group in apiType.GetTypeMembers())
        {
            var endpoints = group.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(static field => field.Name is not BasePathFieldName)
                .Select(static field => new EndpointData(field.Name, GetUrlName(field.Name)))
                .OrderBy(static endpoint => endpoint.UrlName, StringComparer.Ordinal)
                .ToImmutableArray();

            groups.Add(new ApiGroupData(group.Name, endpoints));
        }

        groups.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.TypeName, right.TypeName));

        return new ApiData(
            apiType.ContainingNamespace.ToDisplayString(),
            apiType.Name,
            groups.ToImmutableArray());
    }

    private static string GetUrlName(string pathName) =>
        pathName.EndsWith(PathSuffix, StringComparison.Ordinal)
            ? $"{pathName[..^PathSuffix.Length]}{UrlSuffix}"
            : $"{pathName}{UrlSuffix}";

    private static void Generate(SourceProductionContext spc, ApiData? data)
    {
        if (data is null)
        {
            return;
        }

        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine($"namespace {data.NamespaceName};");
        cb.AppendLine();

        using (cb.Block($"public static partial class {data.TypeName}"))
        {
            for (var i = 0; i < data.Groups.Length; i++)
            {
                var group = data.Groups[i];
                using (cb.Block($"public static partial class {group.TypeName}"))
                {
                    cb.AppendLine(GetGeneratedCodeAttribute(nameof(EuterpeApiUrlGenerator)));
                    cb.AppendLine($"public const string BaseUrl = {data.TypeName}.BaseUrl + {BasePathFieldName};");

                    foreach (var endpoint in group.Endpoints)
                    {
                        cb.AppendLine(GetGeneratedCodeAttribute(nameof(EuterpeApiUrlGenerator)));
                        cb.AppendLine($"public const string {endpoint.UrlName} = BaseUrl + {endpoint.PathName};");
                    }
                }

                if (i < data.Groups.Length - 1)
                {
                    cb.AppendLine();
                }
            }
        }

        spc.AddSource($"{data.TypeName}.Urls.g.cs", cb.ToString());
    }

    private sealed record ApiData(string NamespaceName, string TypeName, ImmutableArray<ApiGroupData> Groups);

    private sealed record ApiGroupData(string TypeName, ImmutableArray<EndpointData> Endpoints);

    private sealed record EndpointData(string PathName, string UrlName);
}
