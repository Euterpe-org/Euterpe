namespace MuseDashModTools.Generators;

public static class Extensions
{
    public static IncrementalValuesProvider<T> WithCondition<T>(
        this IncrementalValuesProvider<T> data,
        IncrementalValueProvider<bool> condition) =>
        data.Combine(condition)
            .Where(static tuple => tuple.Right)
            .Select((tuple, _) => tuple.Left);

    public static IncrementalValueProvider<ImmutableArray<T>> WithCondition<T>(
        this IncrementalValueProvider<ImmutableArray<T>> collectedData,
        IncrementalValueProvider<bool> condition)
    {
        return collectedData
            .Combine(condition)
            .Select(static (tuple, _) =>
                tuple.Right ? tuple.Left : ImmutableArray<T>.Empty);
    }
}