namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    private const string RouteAttributeName = "Euterpe.Shared.Attributes.RouteAttribute";
    private const string PerGameAttributeName = "Euterpe.Shared.Attributes.PerGameAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var routed = context.SyntaxProvider.ForAttributeWithMetadataName(
            RouteAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => ExtractRouted(ctx)).Collect();

        var perGame = context.SyntaxProvider.ForAttributeWithMetadataName(
            PerGameAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => ((INamedTypeSymbol)ctx.TargetSymbol).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).Collect();

        context.RegisterSourceOutput(routed.Combine(perGame), GenerateFromData);
    }

    private static RegistrationData ExtractRouted(GeneratorAttributeSyntaxContext context)
    {
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var attribute = context.Attributes.Single(static x => x.AttributeClass!.ToDisplayString() == RouteAttributeName);
        var path = attribute.ConstructorArguments[0].Value as string ?? string.Empty;
        var isPerGame = symbol.GetAttributes()
            .Any(static a => a.AttributeClass?.ToDisplayString() == PerGameAttributeName);

        return new RegistrationData(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), isPerGame, path);
    }

    private static void GenerateFromData(
        SourceProductionContext spc,
        (ImmutableArray<RegistrationData> Routed, ImmutableArray<string> PerGame) data)
    {
        var (routed, perGame) = data;
        if (routed.IsEmpty && perGame.IsEmpty)
        {
            return;
        }

        var seen = new HashSet<string>();
        var appViewModels = new List<string>();
        var perGameViewModels = new List<string>();

        foreach (var item in routed)
        {
            if (item.Path == "/" || !seen.Add(item.FullName))
            {
                continue;
            }

            (item.IsPerGame ? perGameViewModels : appViewModels).Add(item.FullName);
        }

        foreach (var name in perGame)
        {
            if (seen.Add(name))
            {
                perGameViewModels.Add(name);
            }
        }

        appViewModels.Sort(StringComparer.Ordinal);
        perGameViewModels.Sort(StringComparer.Ordinal);

        var sb = new GeneratorStringBuilder();
        sb.AppendLine("""
                      namespace Euterpe.Extensions;

                      partial class ServiceExtensions
                      {
                      """);

        AppendRegistrationMethod(sb, "RegisterAppViewModels", appViewModels, "SingleInstance()",
            "\t\tbuilder.RegisterType<global::Euterpe.AppViewModel>().PropertiesAutowired().SingleInstance();");
        sb.AppendLine();
        AppendRegistrationMethod(sb, "RegisterPerGameViewModels", perGameViewModels, "InstancePerLifetimeScope()", null);

        sb.AppendLine("}");

        spc.AddSource("ServiceExtensions.g.cs", sb.ToString());
    }

    private static void AppendRegistrationMethod(
        GeneratorStringBuilder sb,
        string methodName,
        List<string> viewModels,
        string lifetime,
        string? prelude)
    {
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void {{methodName}}(this ContainerBuilder builder)
                            {
                        """);

        if (prelude is not null)
        {
            sb.AppendLine(prelude);
            sb.AppendLine();
        }

        foreach (var viewModel in viewModels)
        {
            sb.AppendLine($"\t\tbuilder.RegisterType<{viewModel}>().PropertiesAutowired().{lifetime};");
        }

        sb.AppendLine("    }");
    }

    private sealed record RegistrationData(string FullName, bool IsPerGame, string Path);
}