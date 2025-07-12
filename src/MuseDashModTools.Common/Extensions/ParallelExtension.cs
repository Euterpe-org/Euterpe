namespace MuseDashModTools.Common.Extensions;

public static class ParallelExtension
{
    public static async Task<TResult?[]> WhenAllAsync<TSource, TResult>(this TSource[] source, Func<TSource, Task<TResult?>> action)
    {
        var tasks = new Task<TResult?>[source.Length];
        var processed = 0;

        await Parallel.ForEachAsync(source,
            (file, _) =>
            {
                var index = Interlocked.Increment(ref processed) - 1;
                tasks[index] = action(file);
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}