namespace MuseDashModTools.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class PageNavigateGenerator : IncrementalGeneratorBase
{
    protected override string ExpectedRootNamespace => MuseDashModToolsNamespace;

    protected override void InitializeCore(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<bool> isValidProvider)
    {
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(FilterNode, ExtractDataFromContext);
        context.RegisterSourceOutput(syntaxProvider.WithCondition(isValidProvider), GenerateFromData);
    }

    private static bool FilterNode(SyntaxNode node, CancellationToken _) =>
        node is ClassDeclarationSyntax { BaseList.Types: var types } classDeclarationSyntax
        && types.Any(x => x.Type.ToString() is "NavViewModelBase")
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
            .Single(x => x.Identifier.Text is "NavItems");

        var initializer = propertyDeclaration?.Initializer?.Value;
        if (initializer is not CollectionExpressionSyntax collectionExpression)
        {
            return null;
        }

        var navigateKeys = collectionExpression.Elements
            .Select(element => element.ChildNodes().OfType<ImplicitObjectCreationExpressionSyntax>().Single())
            .Select(node => node.ArgumentList.Arguments[1].ToString())
            .ToArray();

        return new ViewModelData(classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name, navigateKeys);
    }

    private static void GenerateFromData(SourceProductionContext spc, ViewModelData? data)
    {
        if (data is not var (nameSpace, className, navigateKeys))
        {
            return;
        }

        var sb = new IndentedGeneratorStringBuilder();

        sb.AppendLine($$"""
                        namespace {{nameSpace}};

                        partial class {{className}}
                        {
                            {{GetGeneratedCodeAttribute(nameof(PageNavigateGenerator))}}
                            [UsedImplicitly]
                            public required NavigationService NavigationService { get; init; }

                            {{GetGeneratedCodeAttribute(nameof(PageNavigateGenerator))}}
                            protected override void Navigate(NavItem? value)
                            {
                                Content = value?.NavigateKey switch
                                {
                        """);

        sb.IncreaseIndent(3);
        foreach (var navigateKey in navigateKeys)
        {
            sb.AppendLine($"{navigateKey} => NavigationService.NavigateTo<{navigateKey[..^4]}>(),");
        }

        sb.AppendLine("_ => throw new UnreachableException()");

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