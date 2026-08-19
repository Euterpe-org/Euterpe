namespace Euterpe.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RefitJsonContextAnalyzer : DiagnosticAnalyzer
{
    public const string WireTypeRegistrationRuleId = "EUT0005";

    private static readonly DiagnosticDescriptor WireTypeRegistrationRule = new(
        WireTypeRegistrationRuleId,
        "Refit wire types must be registered for JSON serialization",
        "Wire type '{0}' must be registered in SnakeCaseJsonContext",
        "Serialization",
        DiagnosticSeverity.Error,
        true,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [WireTypeRegistrationRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        if (context.Compilation.AssemblyName != "Euterpe.Core")
        {
            return;
        }

        var bodyAttribute = context.Compilation.GetTypeByMetadataName("Refit.BodyAttribute");
        var httpResponseMessage = context.Compilation.GetTypeByMetadataName("System.Net.Http.HttpResponseMessage");
        var jsonSerializableAttribute = context.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonSerializableAttribute");
        var jsonContext = context.Compilation.GetTypeByMetadataName("Euterpe.Core.JsonContexts.SnakeCaseJsonContext");
        var task = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var clientsNamespace = FindNamespace(context.Compilation.GlobalNamespace, "Euterpe", "Core", "Http", "Clients");
        if (bodyAttribute is null
            || clientsNamespace is null
            || httpResponseMessage is null
            || jsonContext is null
            || jsonSerializableAttribute is null
            || task is null)
        {
            return;
        }

        var registeredTypes = jsonContext.GetAttributes()
            .Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, jsonSerializableAttribute))
            .Select(attribute => attribute.ConstructorArguments.FirstOrDefault().Value)
            .OfType<ITypeSymbol>()
            .ToImmutableArray();

        var wireTypes = new Dictionary<ITypeSymbol, Location?>(SymbolEqualityComparer.Default);
        foreach (var client in clientsNamespace.GetTypeMembers().Where(static type => type.TypeKind == TypeKind.Interface))
        {
            foreach (var method in client.GetMembers().OfType<IMethodSymbol>().Where(static method => method.MethodKind == MethodKind.Ordinary))
            {
                if (method.ReturnType is INamedTypeSymbol { IsGenericType: true } returnType
                    && SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, task))
                {
                    wireTypes.TryAdd(returnType.TypeArguments[0], method.Locations.FirstOrDefault());
                }

                foreach (var parameter in method.Parameters.Where(parameter => parameter.GetAttributes()
                             .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, bodyAttribute))))
                {
                    wireTypes.TryAdd(parameter.Type, parameter.Locations.FirstOrDefault());
                }
            }
        }

        foreach (var (wireType, location) in wireTypes)
        {
            if (!SymbolEqualityComparer.Default.Equals(wireType, httpResponseMessage)
                && !registeredTypes.Any(registeredType => SymbolEqualityComparer.Default.Equals(registeredType, wireType)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    WireTypeRegistrationRule,
                    location,
                    wireType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
            }
        }
    }

    private static INamespaceSymbol? FindNamespace(INamespaceSymbol root, params string[] names)
    {
        INamespaceSymbol? current = root;
        foreach (var name in names)
        {
            current = current.GetNamespaceMembers().FirstOrDefault(member => member.Name == name);
            if (current is null)
            {
                return null;
            }
        }

        return current;
    }
}
