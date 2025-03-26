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
        node is ClassDeclarationSyntax { BaseList.Types: var types }
        && types[0].ToString() is "UserControl" or "Window" or "UrsaWindow" or "Application";

    private static ViewData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context.Node is not ClassDeclarationSyntax { BaseList.Types: var types } classDeclaration)
        {
            return null;
        }

        var controlType = ControlType.UserControl;
        var baseTypeName = types[0].ToString();

        controlType = baseTypeName switch
        {
            "UserControl" => ControlType.UserControl,
            "Window" or "UrsaWindow" => ControlType.Window,
            "Application" => ControlType.Application,
            _ => controlType
        };

        return new ViewData(classDeclaration.Identifier.Text, controlType);
    }

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new GeneratorStringBuilder();
        sb.AppendLine($$"""
                        using global::Avalonia.Interactivity;
                        using static global::MuseDashModTools.IocContainer;

                        namespace MuseDashModTools.Extensions;

                        partial class ServiceExtensions
                        {
                            {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                            public static void RegisterViewAndViewModels(this ContainerBuilder builder)
                            {
                        """);

        foreach (var data in dataCollection)
        {
            if (data is not var (name, controlType))
            {
                continue;
            }

            if (controlType is ControlType.Application)
            {
                sb.AppendLine(
                    $"""
                     builder.RegisterType<{name}ViewModel>()
                        .OnActivated(x => new ValueTask(x.Instance.InitializeAsync()))
                        .PropertiesAutowired()
                        .SingleInstance();
                     """);
            }
            else
            {
                sb.AppendLine($"\t\tbuilder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
                GenerateViewRegistration(sb, name, controlType);
            }

            sb.AppendLine();
        }

        sb.AppendLine("""
                          }
                      }
                      """);

        spc.AddSource("ServiceExtensions.g.cs", sb.ToString());
    }

    private static void GenerateViewRegistration(GeneratorStringBuilder sb, string name, ControlType controlType)
    {
        var (eventName, eventArgs) = controlType switch
        {
            ControlType.UserControl => ("Initialized", ""),
            ControlType.Window => ("Loaded", "<RoutedEventArgs>"),
            _ => throw new UnreachableException()
        };

        sb.AppendLine($$"""
                                builder.Register<{{name}}>(ctx => new {{name}}{ DataContext = ctx.Resolve<{{name}}ViewModel>() })
                                .OnActivated(x => Observable.FromEventHandler{{eventArgs}}(
                                        h => x.Instance.{{eventName}} += h,
                                        h => x.Instance.{{eventName}} -= h)
                                    .SubscribeAwait((_, _) => new ValueTask(Resolve<{{name}}ViewModel>().InitializeAsync())))
                                .SingleInstance();
                        """);
    }

    private enum ControlType
    {
        UserControl,
        Window,
        Application
    }

    private sealed record ViewData(string Name, ControlType ControlType);
}