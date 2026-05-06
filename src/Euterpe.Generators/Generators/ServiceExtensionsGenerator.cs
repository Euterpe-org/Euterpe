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
        var perGameViews = views.Where(static d => d.IsPerGame).ToArray();

        var sb = new GeneratorStringBuilder();
        sb.AppendLine("""
                      namespace Euterpe.Extensions;

                      partial class ServiceExtensions
                      {
                      """);

        AppendAppViewsAndViewModels(sb, appViews);
        sb.AppendLine();
        AppendPerGameViews(sb, perGameViews);
        sb.AppendLine();
        AppendPerGameViewModels(sb, perGameViews);

        sb.AppendLine("}");

        spc.AddSource("ServiceExtensions.g.cs", sb.ToString());
    }

    private static void AppendAppViewsAndViewModels(GeneratorStringBuilder sb, IEnumerable<ViewData> views)
    {
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterAppViewsAndViewModels(this ContainerBuilder builder)
                            {
                                builder.RegisterType<global::Euterpe.ViewModels.AppViewModel>().PropertiesAutowired().SingleInstance();

                        """);

        foreach (var (name, _) in views)
        {
            sb.AppendLine($"\t\tbuilder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
            sb.AppendLine($"\t\tbuilder.Register<{name}>(ctx => new {name} {{ DataContext = ctx.Resolve<{name}ViewModel>() }}).SingleInstance();");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
    }

    private static void AppendPerGameViews(GeneratorStringBuilder sb, IEnumerable<ViewData> views)
    {
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterPerGameViews(this ContainerBuilder builder)
                            {
                        """);

        foreach (var (name, _) in views)
        {
            sb.AppendLine($$"""
                                    builder.Register<{{name}}>(_ =>
                                    {
                                        var view = new {{name}}();
                                        global::Euterpe.IocContainer.GameScopeObservable.Subscribe(scope =>
                                        {
                                            var viewModel = scope.Resolve<{{name}}ViewModel>();
                                            view.DataContext = viewModel;
                                            viewModel.InitializeAsync().SafeFireAndForget();
                                        });
                                        return view;
                                    }).SingleInstance();
                            """);
            sb.AppendLine();
        }

        sb.AppendLine("    }");
    }

    private static void AppendPerGameViewModels(GeneratorStringBuilder sb, IEnumerable<ViewData> views)
    {
        sb.AppendLine($$"""
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterPerGameViewModels(this ContainerBuilder builder)
                            {
                        """);

        foreach (var (name, _) in views)
        {
            sb.AppendLine($"\t\tbuilder.RegisterType<{name}ViewModel>().PropertiesAutowired().InstancePerLifetimeScope();");
        }

        sb.AppendLine("    }");
    }

    private static bool HasPerGameAttribute(ClassDeclarationSyntax classDecl) =>
        classDecl.AttributeLists
            .SelectMany(static al => al.Attributes)
            .Any(static attr => attr.Name.ToString() is "PerGameView");

    private sealed record ViewData(string Name, bool IsPerGame);
}