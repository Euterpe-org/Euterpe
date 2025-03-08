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

    protected static IncrementalValuesProvider<T> WithCondition<T>(
        IncrementalValuesProvider<T> data,
        IncrementalValueProvider<bool> condition) =>
        data.Combine(condition)
            .Where(static tuple => tuple.Right)
            .Select((tuple, _) => tuple.Left);

    protected static IncrementalValueProvider<ImmutableArray<T>> WithCollectionCondition<T>(
        IncrementalValueProvider<ImmutableArray<T>> collectedData,
        IncrementalValueProvider<bool> condition)
    {
        return collectedData
            .Combine(condition)
            .Select(static (tuple, _) =>
                tuple.Right ? tuple.Left : ImmutableArray<T>.Empty);
    }
}