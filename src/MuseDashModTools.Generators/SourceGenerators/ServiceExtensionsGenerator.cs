using System.Text;

namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider.CreateSyntaxProvider(
                FilterNode, ExtractDataFromContext).Collect(),
            GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types } &&
        types.Any(x => x.Type.ToString() == "UserControl" || x.Type.ToString().EndsWith("Window"));

    private static ViewData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _) =>
        context.Node is not ClassDeclarationSyntax classDeclaration
            ? null
            : new ViewData(classDeclaration.Identifier.Text);

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<ViewData?> dataList)
    {
        var sb = new StringBuilder();

        foreach (var data in dataList)
        {
            if (data is not var (name))
            {
                continue;
            }

            sb.AppendLine($"builder.Register<{name}>(ctx => new {name} {{ DataContext = ctx.Resolve<{name}ViewModel>() }}).SingleInstance();");
            sb.AppendLine($"builder.RegisterType<{name}ViewModel>().PropertiesAutowired().SingleInstance();");
            sb.AppendLine();
        }

        spc.AddSource("ServiceExtensions.g.cs",
            Header +
            $$"""
              namespace MuseDashModTools.Extensions;

              partial class ServiceExtensions
              {
                  {{GetGeneratedCodeAttribute(nameof(ServiceExtensionsGenerator))}}
                  public static void RegisterViewAndViewModels(this ContainerBuilder builder)
                  {
                      {{sb.ToString().TrimEnd()}}
                  }
              }
              """
        );
    }

    private sealed record ViewData(string Name);
}