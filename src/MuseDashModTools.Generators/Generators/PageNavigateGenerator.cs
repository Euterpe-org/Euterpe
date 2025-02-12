namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class PageNavigateGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            context.SyntaxProvider.CreateSyntaxProvider(
                FilterNode,
                ExtractDataFromContext),
            GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types } classDeclarationSyntax
        && types.Any(x => x.Type.ToString() is "PageViewModelBase" or "NavViewModelBase")
        && classDeclarationSyntax.Identifier.Text.EndsWith("ViewModel");

    private static ViewModelData? ExtractDataFromContext(GeneratorSyntaxContext context, CancellationToken _)
    {
        if (context is not
            {
                Node: ClassDeclarationSyntax classDeclaration,
                SemanticModel: var semanticModel
            })
        {
            return null;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
        {
            return null;
        }

        var propertyDeclaration = classDeclaration.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Single(x => x.Identifier.Text == "NavItems");

        var initializer = propertyDeclaration?.Initializer?.Value;
        if (initializer is not CollectionExpressionSyntax collectionExpression)
        {
            return null;
        }

        var navigateKeys = collectionExpression.Elements
            .Select(element => element.ChildNodes().OfType<ImplicitObjectCreationExpressionSyntax>().Single())
            .Select(node => node.ArgumentList.Arguments[1].ToString())
            .ToArray();

        return new(classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name, navigateKeys);
    }

    private static void GenerateFromData(SourceProductionContext spc, ViewModelData? data)
    {
        if (data is not var (nameSpace, className, navigateKeys))
        {
            return;
        }

        var sb = new IndentedStringBuilder();

        sb.AppendLine(Header);
        sb.AppendLine($$"""
                        namespace {{nameSpace}};

                        partial class {{className}}
                        {
                            {{GetGeneratedCodeAttribute(nameof(PageNavigateGenerator))}}
                            protected override void Navigate(NavItem? value)
                            {
                                Content = value?.NavigateKey switch
                                {
                        """);

        sb.IncreaseIndent(3);
        foreach (var navigateKey in navigateKeys)
        {
            sb.AppendLine($"\t{navigateKey} => NavigationService.NavigateTo<{navigateKey[..^4]}>(),");
        }

        sb.AppendLine("""
                      _ => throw new UnreachableException()
                      """
        );

        sb.ResetIndent();
        sb.AppendLine("""
                              };
                          }
                      }
                      """);
        spc.AddSource($"{className}.Navigate.g.cs", sb.ToString());
    }

    private sealed record ViewModelData(string NameSpace, string ClassName, string[] NavigateKeys);
}