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
        context.Node is not ClassDeclarationSyntax classDeclaration
            ? null
            : new ViewData(classDeclaration.Identifier.Text, HasPerGameAttribute(classDeclaration));

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var views = dataCollection.OfType<ViewData>().ToArray();
        var appViews = views.Where(static d => !d.IsPerGame);
        var perGameViews = views.Where(static d => d.IsPerGame);

        var sb = new GeneratorStringBuilder();
        sb.AppendLine($$"""
                        namespace Euterpe.Extensions;

                        partial class ServiceExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterAppViewsAndViewModels(this ContainerBuilder builder)
                            {
                                builder.RegisterType<global::Euterpe.ViewModels.AppViewModel>().PropertiesAutowired().SingleInstance();

                        """);

        foreach (var (name, _) in appViews)
        {
            sb.AppendLine($"\t\tbuilder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
            sb.AppendLine($"\t\tbuilder.Register<{name}>(ctx => new {name} {{ DataContext = ctx.Resolve<{name}ViewModel>() }}).SingleInstance();");
            sb.AppendLine();
        }

        sb.AppendLine($$"""
                            }

                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterPerGameViewsAndViewModels(this ContainerBuilder builder)
                            {
                        """);

        foreach (var (name, _) in perGameViews)
        {
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

    private static bool HasPerGameAttribute(ClassDeclarationSyntax classDecl) =>
        classDecl.AttributeLists
            .SelectMany(static al => al.Attributes)
            .Any(static attr => attr.Name.ToString() is "PerGameView");

    private sealed record ViewData(string Name, bool IsPerGame);
}