namespace Euterpe.Generators.Extensions;

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

    public static IncrementalValueProvider<(ImmutableArray<T1>, ImmutableArray<T2>)> WithCondition<T1, T2>(
        this IncrementalValueProvider<(ImmutableArray<T1> Left, ImmutableArray<T2> Right)> combinedData,
        IncrementalValueProvider<bool> condition)
    {
        return combinedData
            .Combine(condition)
            .Select(static (tuple, _) =>
                tuple.Right
                    ? tuple.Left
                    : (ImmutableArray<T1>.Empty, ImmutableArray<T2>.Empty));
    }
}