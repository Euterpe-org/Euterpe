namespace Euterpe.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            FilterNode, ExtractDataFromContext).Collect();
        context.RegisterSourceOutput(syntaxProvider, GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types }
        && types[0].ToString() is "UserControl" or "SplashWindow" or "UrsaWindow";

    private static ViewData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _) =>
        context.Node is not ClassDeclarationSyntax classDeclaration ? null : new ViewData(classDeclaration.Identifier.Text);

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new GeneratorStringBuilder();
        sb.AppendLine($$"""
                        namespace Euterpe.Extensions;

                        partial class ServiceExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterViewAndViewModels(this ContainerBuilder builder)
                            {
                                builder.RegisterType<global::Euterpe.ViewModels.AppViewModel>().PropertiesAutowired().SingleInstance();

                        """);

        foreach (var data in dataCollection)
        {
            if (data is not var (name))
            {
                continue;
            }

            sb.AppendLine($"\t\tbuilder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
            sb.AppendLine($"\t\tbuilder.Register<{name}>(ctx => new {name} {{ DataContext = ctx.Resolve<{name}ViewModel>() }}).SingleInstance();");
            sb.AppendLine();
        }

        sb.AppendLine("""
                          }
                      }
                      """);

        spc.AddSource("ServiceExtensions.g.cs", sb.ToString());
    }

    private sealed record ViewData(string Name);
}