namespace Euterpe.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArchitectureAnalyzer : DiagnosticAnalyzer
{
    public const string AbstractionsTypeRuleId = "EUT0001";
    public const string CoreTypeRuleId = "EUT0002";
    public const string SharedTypeRuleId = "EUT0003";
    public const string ModelsTypeRuleId = "EUT0004";

    private static readonly DiagnosticDescriptor AbstractionsTypeRule = new(
        AbstractionsTypeRuleId,
        "Abstractions types must be public interfaces",
        "Type '{0}' in Euterpe.Abstractions must be a public interface",
        "Architecture",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor CoreTypeRule = new(
        CoreTypeRuleId,
        "Core services must be internal and sealed",
        "Type '{0}' in Euterpe.Core must be internal and sealed",
        "Architecture",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor SharedTypeRule = new(
        SharedTypeRuleId,
        "Shared classes must be public",
        "Type '{0}' in Euterpe.Shared must be public",
        "Architecture",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor ModelsTypeRule = new(
        ModelsTypeRuleId,
        "Model classes must be public",
        "Type '{0}' in Euterpe.Models must be public",
        "Architecture",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [AbstractionsTypeRule, CoreTypeRule, SharedTypeRule, ModelsTypeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.IsImplicitlyDeclared || type.ContainingType is not null)
        {
            return;
        }

        var assemblyName = type.ContainingAssembly.Name;
        var namespaceName = type.ContainingNamespace.ToDisplayString();
        var rule = assemblyName switch
        {
            "Euterpe.Abstractions" when namespaceName == "Euterpe.Abstractions"
                && (type.TypeKind != TypeKind.Interface || type.DeclaredAccessibility != Accessibility.Public) => AbstractionsTypeRule,
            "Euterpe.Core" when namespaceName == "Euterpe.Core"
                && type.TypeKind == TypeKind.Class
                && !type.IsStatic
                && (type.DeclaredAccessibility != Accessibility.Internal || !type.IsSealed) => CoreTypeRule,
            "Euterpe.Shared" when namespaceName == "Euterpe.Shared"
                && type.TypeKind == TypeKind.Class
                && type.DeclaredAccessibility != Accessibility.Public => SharedTypeRule,
            "Euterpe.Models" when namespaceName == "Euterpe.Models"
                && type.TypeKind == TypeKind.Class
                && !type.IsStatic
                && type.DeclaredAccessibility != Accessibility.Public => ModelsTypeRule,
            _ => null
        };

        if (rule is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, type.Locations.FirstOrDefault(), type.Name));
        }
    }
}
