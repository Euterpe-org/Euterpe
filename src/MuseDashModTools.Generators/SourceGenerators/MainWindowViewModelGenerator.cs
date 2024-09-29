using System.Text;

namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class MainWindowViewModelGenerator : IIncrementalGenerator
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
        var sb = new StringBuilder();

        foreach (var data in dataList)
        {
            if (data is not var (name))
            {
                continue;
            }

            sb.AppendLine($"case {name}Name:");
            sb.AppendLine($"\tNavigationService.NavigateTo<{name}>();");
            sb.AppendLine("\tbreak;");
        }

        spc.AddSource("MainWindowViewModel.g.cs",
            Header +
            $$"""
              namespace MuseDashModTools.ViewModels;

              partial class MainWindowViewModel
              {
                  {{GetGeneratedCodeAttribute(nameof(MainWindowViewModelGenerator))}}
                  public void Receive(SelectedPageChangedMessage message)
                  {
                      switch (message.Value)
                      {
                          {{sb.ToString().TrimEnd()}}
                      }
                  }
              }
              """
        );
    }

    private sealed record PageData(string Name);
}