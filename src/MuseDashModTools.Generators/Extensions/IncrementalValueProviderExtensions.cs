namespace MuseDashModTools.Generators.Extensions;

public static class IncrementalValueProviderExtensions
{
    public static IncrementalValuesProvider<TSource> WithCondition<TSource>(
        this IncrementalValuesProvider<TSource> data,
        IncrementalValueProvider<bool> condition) =>
        data.Combine(condition)
            .Where(static tuple => tuple.Right)
            .Select((tuple, _) => tuple.Left);

    public static IncrementalValueProvider<ImmutableArray<TSource>> WithCondition<TSource>(
        this IncrementalValueProvider<ImmutableArray<TSource>> collectedData,
        IncrementalValueProvider<bool> condition)
    {
        return collectedData
            .Combine(condition)
            .Select(static (tuple, _) =>
                tuple.Right ? tuple.Left : ImmutableArray<TSource>.Empty);
    }
}