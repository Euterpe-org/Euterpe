namespace Euterpe.Shared.Extensions;

public static class EnumerableTaskExtensions
{
    extension<TSource>(IEnumerable<TSource> source)
    {
        public Task<TResult[]> WhenAllAsync<TResult>(Func<TSource, Task<TResult>> selector)
            => Task.WhenAll(source.Select(selector));
    }
}