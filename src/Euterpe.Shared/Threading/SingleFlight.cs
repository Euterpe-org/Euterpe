using System.Collections.Concurrent;

namespace Euterpe.Shared.Threading;

/// <summary>
///     Coordinates asynchronous work so that at most one task runs per key.
///     Concurrent callers with the same key await the same underlying task;
///     the slot is released once the task completes, so the next call starts fresh.
/// </summary>
/// <typeparam name="TKey">The key type used to identify in-flight work.</typeparam>
public sealed class SingleFlight<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<Task>> _running = new();

    public Task RunAsync(TKey key, Func<Task> work)
    {
        var lazy = _running.GetOrAdd(
            key,
            static (k, state) => new Lazy<Task>(
                () => state.self.RunCoreAsync(k, state.work),
                LazyThreadSafetyMode.ExecutionAndPublication),
            (self: this, work));
        return lazy.Value;
    }

    // Concurrent callers joining a key must all use the same result type; the shared task is cast back to Task<TResult>.
    public Task<TResult> RunAsync<TResult>(TKey key, Func<Task<TResult>> work)
    {
        var lazy = _running.GetOrAdd(
            key,
            static (k, state) => new Lazy<Task>(
                () => state.self.RunCoreAsync(k, state.work),
                LazyThreadSafetyMode.ExecutionAndPublication),
            (self: this, work));
        return (Task<TResult>)lazy.Value;
    }

    private async Task RunCoreAsync(TKey key, Func<Task> work)
    {
        try
        {
            await work().ConfigureAwait(false);
        }
        finally
        {
            _running.TryRemove(key, out _);
        }
    }

    private async Task<TResult> RunCoreAsync<TResult>(TKey key, Func<Task<TResult>> work)
    {
        try
        {
            return await work().ConfigureAwait(false);
        }
        finally
        {
            _running.TryRemove(key, out _);
        }
    }
}
