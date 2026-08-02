using Lock = System.Threading.Lock;
using Timeout = System.Threading.Timeout;

namespace Euterpe.Shared.Threading;

public sealed class DebouncedAsyncAction : IDisposable
{
    private readonly Func<Task> _actionAsync;
    private readonly TimeSpan _debounce;
    private readonly ITimer _debounceTimer;
    private readonly Action<Exception> _exceptionHandler;
    private readonly Lock _gate = new();
    private bool _disposed;
    private bool _rerunRequested;
    private bool _running;

    public DebouncedAsyncAction(
        TimeSpan debounce,
        Func<Task> actionAsync,
        Action<Exception> exceptionHandler,
        TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(debounce, TimeSpan.Zero);
        ArgumentNullException.ThrowIfNull(actionAsync);
        ArgumentNullException.ThrowIfNull(exceptionHandler);

        _debounce = debounce;
        _actionAsync = actionAsync;
        _exceptionHandler = exceptionHandler;
        _debounceTimer = (timeProvider ?? TimeProvider.System).CreateTimer(
            static state => ((DebouncedAsyncAction)state!).OnDebounceElapsed(),
            this,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan);
    }

    public void Trigger()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _debounceTimer.Change(_debounce, Timeout.InfiniteTimeSpan);
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        _debounceTimer.Dispose();
    }

    private void OnDebounceElapsed()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            if (_running)
            {
                _rerunRequested = true;
                return;
            }

            _running = true;
        }

        _ = RunActionAsync();
    }

    private async Task RunActionAsync()
    {
        try
        {
            try
            {
                await _actionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _exceptionHandler(ex);
            }
        }
        finally
        {
            CompleteRun();
        }
    }

    private void CompleteRun()
    {
        bool rerun;
        lock (_gate)
        {
            _running = false;
            rerun = _rerunRequested;
            _rerunRequested = false;
        }

        if (rerun)
        {
            Trigger();
        }
    }
}
