namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IncrementalGeneratorBase
{
    protected override string ExpectedRootNamespace => MuseDashModToolsNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            FilterNode, ExtractDataFromContext).Collect();
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types } &&
        types.Any(x => x.Type.ToString().Contains("UserControl") || x.Type.ToString().Contains("Window"));

    private static ViewData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _) =>
        context.Node is not ClassDeclarationSyntax { BaseList.Types: var types } classDeclaration
            ? null
            : new ViewData(classDeclaration.Identifier.Text, types.Any(x => x.Type.ToString().Contains("Window")));

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new IndentedStringBuilder();
        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        using global::Avalonia.Interactivity;

                        namespace MuseDashModTools.Extensions;

                        partial class ServiceExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterViewAndViewModels(this ContainerBuilder builder)
                            {
                        """);

        sb.IncreaseIndent(2);
        foreach (var data in dataCollection)
        {
            if (data is not var (name, isWindow))
            {
                continue;
            }

            var (eventName, eventArgs) = isWindow
                ? ("Loaded", "<RoutedEventArgs>")
                : ("Initialized", "");

            sb.AppendLine($$"""
                            builder.Register<{{name}}>(ctx => new {{name}}{ DataContext = ctx.Resolve<{{name}}ViewModel>() })
                            .OnActivated(x => Observable.FromEventHandler{{eventArgs}}(
                                    h => x.Instance.{{eventName}} += h,
                                    h => x.Instance.{{eventName}} -= h)
                                .SubscribeAwait((_, _) => new ValueTask(App.Container.Resolve<{{name}}ViewModel>().InitializeAsync())))
                            .SingleInstance();
                            """);

            sb.AppendLine($"builder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
            sb.AppendLine();
        }

        sb.ResetIndent();
        sb.AppendLine("""
                          }
                      }
                      """);

        spc.AddSource("ServiceExtensions.g.cs", sb.ToString());
    }

    private sealed record ViewData(string Name, bool IsWindow);
}