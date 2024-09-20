namespace MuseDashModToolsUI.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class NavigationServiceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider.CreateSyntaxProvider(
                FilterNode, ExtractDataFromContext).Collect(),
            GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types } classDeclarationSyntax
        && types.Any(x => x.Type.ToString() == "UserControl")
        && classDeclarationSyntax.Identifier.Text.EndsWith("Page");

    private static PageData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _) =>
        context.Node is not ClassDeclarationSyntax classDeclaration
            ? null
            : new PageData(classDeclaration.Identifier.Text);

    private static void GenerateFromData(SourceProductionContext spc, ImmutableArray<PageData?> dataList)
    {
        spc.AddSource("NavigationService.g.cs",
            Header +
            $$"""
              namespace MuseDashModToolsUI.Services;

              partial class NavigationService
              {
                  {{GetGeneratedCodeAttribute(nameof(NavigationServiceGenerator))}}
                  private readonly Dictionary<Type, Type> _viewModelToViewMap = new()
                  {
                      {{string.Join(",\r\n", dataList.Select(data => $"{{ typeof({data?.Name}ViewModel), typeof({data?.Name}) }}"))}}
                  };
              }
              """
        );
    }

    private sealed record PageData(string Name);
}