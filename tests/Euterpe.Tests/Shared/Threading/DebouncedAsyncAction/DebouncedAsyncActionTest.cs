using Euterpe.Shared.Threading;

namespace Euterpe.Tests.Shared.Threading;

[Category("DebouncedAsyncActionTests")]
[TestSubject(typeof(DebouncedAsyncAction))]
public sealed partial class DebouncedAsyncActionTest
{
    private static readonly TimeSpan ActionTimeout = TimeSpan.FromSeconds(10);

    private static DebouncedAsyncAction Create(
        Func<Task> actionAsync,
        out ManualDebounceTimer timer,
        Action<Exception>? exceptionHandler = null)
    {
        var timeProvider = new ManualTimeProvider();
        var action = new DebouncedAsyncAction(
            TimeSpan.Zero,
            actionAsync,
            exceptionHandler ?? (static _ => { }),
            timeProvider);

        timer = timeProvider.Timer;
        return action;
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        public ManualDebounceTimer Timer { get; private set; } = null!;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Timer = new ManualDebounceTimer(callback, state);
            Timer.Change(dueTime, period);
            return Timer;
        }
    }

    private sealed class ManualDebounceTimer(TimerCallback callback, object? state) : ITimer
    {
        private readonly Lock _gate = new();
        private int _isScheduled;
        private TaskCompletionSource _nextSchedule = NewScheduleSource();

        public bool IsScheduled => Volatile.Read(ref _isScheduled) != 0;

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (dueTime == Timeout.InfiniteTimeSpan)
            {
                Interlocked.Exchange(ref _isScheduled, 0);
                return true;
            }

            TaskCompletionSource scheduled;
            lock (_gate)
            {
                Interlocked.Exchange(ref _isScheduled, 1);
                scheduled = _nextSchedule;
                _nextSchedule = NewScheduleSource();
            }

            scheduled.TrySetResult();
            return true;
        }

        public void Elapse()
        {
            if (Interlocked.Exchange(ref _isScheduled, 0) == 0)
            {
                throw new InvalidOperationException("The debounce timer is not scheduled");
            }

            callback(state);
        }

        public void Dispose() => Interlocked.Exchange(ref _isScheduled, 0);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public Task WaitForNextScheduleAsync()
        {
            lock (_gate)
            {
                return IsScheduled ? Task.CompletedTask : _nextSchedule.Task;
            }
        }

        private static TaskCompletionSource NewScheduleSource() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
