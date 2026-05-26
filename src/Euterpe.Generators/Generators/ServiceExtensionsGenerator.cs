namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";
    private const string PerGameAttributeName = "Euterpe.Shared.Attributes.PerGameAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var routed = context.SyntaxProvider
            .ForAttributeWithMetadataName(RouteAttributeName, static (node, _) => node is ClassDeclarationSyntax, ExtractRouted)
            .Collect();

        var perGame = context.SyntaxProvider
            .ForAttributeWithMetadataName(PerGameAttributeName, static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => ctx.TargetSymbol is INamedTypeSymbol symbol ? symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : string.Empty)
            .Collect();

        context.RegisterSourceOutput(routed.Combine(perGame), Generate);
    }

    private static RegistrationData? ExtractRouted(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                TargetSymbol: INamedTypeSymbol symbol,
                Attributes:
                [
                    { ConstructorArguments: [{ Value: string path }] }
                ]
            })
        {
            return null;
        }

        var isPerGame = symbol.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() is PerGameAttributeName);
        return new RegistrationData(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), isPerGame, path);
    }

    private static void Generate(SourceProductionContext spc, (ImmutableArray<RegistrationData?> Routed, ImmutableArray<string> PerGame) data)
    {
        var (routed, perGame) = data;
        if (routed.IsEmpty && perGame.IsEmpty)
        {
            return;
        }

        var registered = new HashSet<string>();
        var appViewModels = new List<string>();
        var perGameViewModels = new List<string>();

        foreach (var item in routed)
        {
            if (item is not { Path: not "/" } || !registered.Add(item.FullName))
            {
                continue;
            }

            (item.IsPerGame ? perGameViewModels : appViewModels).Add(item.FullName);
        }

        perGameViewModels.AddRange(perGame.Where(registered.Add));

        appViewModels.Sort(StringComparer.Ordinal);
        perGameViewModels.Sort(StringComparer.Ordinal);

        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("namespace Euterpe.Extensions;");
        cb.AppendLine();

        using (cb.Block("partial class ServiceExtensions"))
        {
            AppendRegistration(cb, "RegisterAppViewModels", "SingleInstance()", appViewModels, true);
            cb.AppendLine();
            AppendRegistration(cb, "RegisterPerGameViewModels", "InstancePerLifetimeScope()", perGameViewModels, false);
        }

        spc.AddSource("ServiceExtensions.g.cs", cb.ToString());
    }

    private static void AppendRegistration(CodeBuilder cb, string methodName, string lifetime, List<string> viewModels, bool registerAppViewModel)
    {
        cb.AppendLine(GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator)));
        using (cb.Block($"public static void {methodName}(this ContainerBuilder builder)"))
        {
            if (registerAppViewModel)
            {
                cb.AppendLine("builder.RegisterType<global::Euterpe.AppViewModel>().PropertiesAutowired().SingleInstance();");
                cb.AppendLine();
            }

            foreach (var viewModel in viewModels)
            {
                cb.AppendLine($"builder.RegisterType<{viewModel}>().PropertiesAutowired().{lifetime};");
            }
        }
    }

    private sealed record RegistrationData(string FullName, bool IsPerGame, string Path);
}