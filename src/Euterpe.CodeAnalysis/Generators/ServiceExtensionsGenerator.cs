namespace Euterpe.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";
    private const string RegisterAttributeName = "Euterpe.Shared.Attributes.RegisterAttribute";
    private const string AppSingletonAttributeName = "Euterpe.Shared.Attributes.AppSingletonAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var routed = context.SyntaxProvider
            .ForAttributeWithMetadataName(RouteAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractRegistration)
            .Collect();

        var registered = context.SyntaxProvider
            .ForAttributeWithMetadataName(RegisterAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractRegistration)
            .Collect();

        context.RegisterSourceOutput(routed.Combine(registered), Generate);
    }

    private static RegistrationData? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        var isAppSingleton = symbol.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() is AppSingletonAttributeName);
        return new RegistrationData(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), isAppSingleton);
    }

    private static void Generate(SourceProductionContext spc,
        (ImmutableArray<RegistrationData?> Routed, ImmutableArray<RegistrationData?> Registered) data)
    {
        var (routed, registered) = data;
        if (routed.IsEmpty && registered.IsEmpty)
        {
            return;
        }

        var seen = new HashSet<string>();
        var appViewModels = new List<string>();
        var perGameViewModels = new List<string>();

        Collect(routed);
        Collect(registered);

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

        void Collect(ImmutableArray<RegistrationData?> items)
        {
            foreach (var item in items)
            {
                if (item is null || !seen.Add(item.FullName))
                {
                    continue;
                }

                (item.IsAppSingleton ? appViewModels : perGameViewModels).Add(item.FullName);
            }
        }
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

    private sealed record RegistrationData(string FullName, bool IsAppSingleton);
}
