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
        && types[0].ToString() is var baseTypeName
        && (baseTypeName.Contains("UserControl")
            || baseTypeName.Contains("Window")
            || baseTypeName.Contains("Application"));

    private static ViewData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context.Node is not ClassDeclarationSyntax { BaseList.Types: var types } classDeclaration)
        {
            return null;
        }

        var controlType = ControlType.UserControl;
        var baseTypeName = types[0].ToString();

        if (baseTypeName.Contains("UserControl"))
        {
            controlType = ControlType.UserControl;
        }
        else if (baseTypeName.Contains("Window"))
        {
            controlType = ControlType.Window;
        }
        else if (baseTypeName.Contains("Application"))
        {
            controlType = ControlType.Application;
        }

        return new ViewData(classDeclaration.Identifier.Text, controlType);
    }

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataCollection)
    {
        if (dataCollection is [])
        {
            return;
        }

        var sb = new StringBuilder();
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

        foreach (var data in dataCollection)
        {
            if (data is not var (name, controlType))
            {
                continue;
            }

            if (controlType is ControlType.Application)
            {
                sb.AppendLine(
                    $"builder.RegisterType<{name}ViewModel>().OnActivated(x => new ValueTask(x.Instance.InitializeAsync())).PropertiesAutowired().SingleInstance();");
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

    private static void GenerateViewRegistration(StringBuilder sb, string name, ControlType controlType)
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
                                    .SubscribeAwait((_, _) => new ValueTask(App.Container.Resolve<{{name}}ViewModel>().InitializeAsync())))
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