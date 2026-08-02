namespace Euterpe.Tests.Shared.Threading;

public sealed partial class DebouncedAsyncActionTest
{
    [Test]
    public async Task Trigger_BurstBeforeDebounce_RunsActionOnce()
    {
        var actionCount = 0;
        using var action = Create(
            () =>
            {
                Interlocked.Increment(ref actionCount);
                return Task.CompletedTask;
            },
            out var timer);

        for (var i = 0; i < 5; i++)
        {
            action.Trigger();
        }

        timer.Elapse();

        await Assert.That(actionCount).IsEqualTo(1);
        await Assert.That(timer.IsScheduled).IsFalse();
    }

    [Test]
    public async Task Trigger_AfterDispose_DoesNotScheduleAction()
    {
        var action = Create(static () => Task.CompletedTask, out var timer);

        action.Dispose();
        action.Trigger();

        await Assert.That(timer.IsScheduled).IsFalse();
    }
}
