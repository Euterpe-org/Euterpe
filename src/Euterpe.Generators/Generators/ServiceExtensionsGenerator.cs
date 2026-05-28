namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";
    private const string PerGameAttributeName = "Euterpe.Shared.Attributes.PerGameAttribute";
    private const string RegisterAttributeName = "Euterpe.Shared.Attributes.RegisterAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var routed = context.SyntaxProvider
            .ForAttributeWithMetadataName(RouteAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractRouted)
            .Collect();

        var perGame = context.SyntaxProvider
            .ForAttributeWithMetadataName(PerGameAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractFullName)
            .Collect();

        var registered = context.SyntaxProvider
            .ForAttributeWithMetadataName(RegisterAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractFullName)
            .Collect();

        context.RegisterSourceOutput(routed.Combine(perGame).Combine(registered), Generate);
    }

    private static string ExtractFullName(GeneratorAttributeSyntaxContext context, CancellationToken _) =>
        context.TargetSymbol is INamedTypeSymbol symbol ? symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : string.Empty;

    private static RegistrationData? ExtractRouted(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        var isPerGame = symbol.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() is PerGameAttributeName);
        return new RegistrationData(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), isPerGame);
    }

    private static void Generate(SourceProductionContext spc, ((ImmutableArray<RegistrationData?> Routed, ImmutableArray<string> PerGame) RoutedAndPerGame, ImmutableArray<string> Registered) data)
    {
        var ((routed, perGame), registered) = data;
        if (routed.IsEmpty && perGame.IsEmpty && registered.IsEmpty)
        {
            return;
        }

        var seen = new HashSet<string>();
        var appViewModels = new List<string>();
        var perGameViewModels = new List<string>();

        foreach (var item in routed)
        {
            if (item is null || !seen.Add(item.FullName))
            {
                continue;
            }

            (item.IsPerGame ? perGameViewModels : appViewModels).Add(item.FullName);
        }

        appViewModels.AddRange(registered.Where(seen.Add));
        perGameViewModels.AddRange(perGame.Where(seen.Add));

        appViewModels.Sort(StringComparer.Ordinal);
        perGameViewModels.Sort(StringComparer.Ordinal);

        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("namespace Euterpe.Extensions;");
        cb.AppendLine();

        using (cb.Block("partial class ServiceExtensions"))
        {
            AppendRegistration(cb, "RegisterAppViewModels", "SingleInstance()", appViewModels);
            cb.AppendLine();
            AppendRegistration(cb, "RegisterPerGameViewModels", "InstancePerLifetimeScope()", perGameViewModels);
        }

        spc.AddSource("ServiceExtensions.g.cs", cb.ToString());
    }

    private static void AppendRegistration(CodeBuilder cb, string methodName, string lifetime, List<string> viewModels)
    {
        cb.AppendLine(GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator)));
        using (cb.Block($"public static void {methodName}(this ContainerBuilder builder)"))
        {
            foreach (var viewModel in viewModels)
            {
                cb.AppendLine($"builder.RegisterType<{viewModel}>().PropertiesAutowired().{lifetime};");
            }
        }
    }

    private sealed record RegistrationData(string FullName, bool IsPerGame);
}