using System.Collections.Frozen;

namespace Euterpe.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public sealed class PlatformServiceExtensionsGenerator : IIncrementalGenerator
{
    private const string PlatformServiceAttributeName = "Euterpe.Shared.Attributes.PlatformServiceAttribute";
    private const string SupportedOSPlatformAttributeName = "System.Runtime.Versioning.SupportedOSPlatformAttribute";

    private static readonly FrozenDictionary<string, string> PreprocessorSymbols =
        new Dictionary<string, string>
        {
            ["Windows"] = "WINDOWS",
            ["Linux"] = "LINUX",
            ["OSX"] = "MACOS"
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var registrations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SupportedOSPlatformAttributeName,
                static (node, _) => node is ClassDeclarationSyntax,
                ExtractRegistration)
            .Collect();

        context.RegisterSourceOutput(registrations, Generate);
    }

    private static RegistrationData? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        var implementation = (INamedTypeSymbol)context.TargetSymbol;
        var platform = (string)context.Attributes[0].ConstructorArguments[0].Value!;
        var preprocessorSymbol = PreprocessorSymbols[platform];

        foreach (var contract in implementation.Interfaces)
        {
            var attribute = contract.GetAttributes().FirstOrDefault(static attribute =>
                attribute.AttributeClass?.ToDisplayString() is PlatformServiceAttributeName);

            if (attribute is null)
            {
                continue;
            }

            return new RegistrationData(
                implementation.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                contract.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                preprocessorSymbol,
                attribute.ConstructorArguments is [{ Value: 0 }]);
        }

        return null;
    }

    private static void Generate(SourceProductionContext spc, ImmutableArray<RegistrationData?> candidates)
    {
        var registrations = candidates
            .OfType<RegistrationData>()
            .OrderBy(static registration => registration.ContractFullName, StringComparer.Ordinal)
            .ToArray();

        if (registrations.Length is 0)
        {
            return;
        }

        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("namespace Euterpe.Core.Extensions;");
        cb.AppendLine();

        using (cb.Block("public static partial class CoreServiceExtensions"))
        using (cb.Block("extension(ContainerBuilder builder)"))
        {
            AppendRegistrationMethod(
                cb,
                "RegisterPerPlatformAppServices",
                "SingleInstance()",
                registrations.Where(static registration => registration.IsAppSingleton));
            cb.AppendLine();
            AppendRegistrationMethod(
                cb,
                "RegisterPerPlatformGameServices",
                "InstancePerLifetimeScope()",
                registrations.Where(static registration => !registration.IsAppSingleton));
        }

        spc.AddSource("CoreServiceExtensions.PlatformServices.g.cs", cb.ToString());
    }

    private static void AppendRegistrationMethod(
        CodeBuilder cb,
        string methodName,
        string lifetime,
        IEnumerable<RegistrationData> registrations)
    {
        var registrationsByPlatform = registrations.ToLookup(static registration => registration.PreprocessorSymbol);

        cb.AppendLine(GetGeneratedCodeAttribute(nameof(PlatformServiceExtensionsGenerator)));
        using (cb.Block($"private void {methodName}()"))
        {
            cb.AppendLine("#pragma warning disable CA1416");
            var directive = "#if";
            foreach (var preprocessorSymbol in PreprocessorSymbols.Values.OrderBy(static symbol => symbol, StringComparer.Ordinal))
            {
                cb.AppendLine($"{directive} {preprocessorSymbol}");
                directive = "#elif";

                foreach (var registration in registrationsByPlatform[preprocessorSymbol])
                {
                    cb.AppendLine($"builder.RegisterType<{registration.ImplementationFullName}>().As<{registration.ContractFullName}>().PropertiesAutowired().{lifetime};");
                }
            }

            cb.AppendLine("#endif");
            cb.AppendLine("#pragma warning restore CA1416");
        }
    }

    private sealed record RegistrationData(
        string ImplementationFullName,
        string ContractFullName,
        string PreprocessorSymbol,
        bool IsAppSingleton);
}
