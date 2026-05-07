namespace Euterpe.Generators.Tests;

internal static class GeneratorTestHelper
{
    public static GeneratorDriver Run<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(source)],
            Net100.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return CSharpGeneratorDriver.Create(new TGenerator().AsSourceGenerator()).RunGenerators(compilation);
    }
}