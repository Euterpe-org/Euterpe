namespace MuseDashModTools.Generators;

public abstract class IncrementalGeneratorBase : IIncrementalGenerator
{
    protected abstract string? ExpectedRootNamespace { get; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNamespaceProvider = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
                provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns) ? ns : null);

        var isValidProvider = rootNamespaceProvider
            .Select((ns, _) => ExpectedRootNamespace is null || ns == ExpectedRootNamespace);

        InitializeCore(context, isValidProvider);
    }

    protected abstract void InitializeCore(
        IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<bool> isValidProvider);
}