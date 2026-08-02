namespace Euterpe.Tests.Shared.Threading;

public sealed partial class DebouncedAsyncActionTest
{
    [Test]
    public async Task RunActionAsync_ActionThrows_NextTriggerStillRunsAction()
    {
        var actionCount = 0;
        var exceptionReported = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var action = Create(
            () =>
            {
                if (Interlocked.Increment(ref actionCount) == 1)
                {
                    throw new InvalidOperationException("action failed");
                }

                return Task.CompletedTask;
            },
            out var timer,
            exception => exceptionReported.TrySetResult(exception));

        action.Trigger();
        timer.Elapse();
        var exception = await exceptionReported.Task.WaitAsync(ActionTimeout).ConfigureAwait(false);

        action.Trigger();
        timer.Elapse();

        await Assert.That(exception.Message).IsEqualTo("action failed");
        await Assert.That(actionCount).IsEqualTo(2);
    }

    [Test]
    public async Task RunActionAsync_TriggerDuringAction_RunsActionAgain()
    {
        var actionCount = 0;
        var firstActionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstActionGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var action = Create(
            async () =>
            {
                if (Interlocked.Increment(ref actionCount) == 1)
                {
                    firstActionStarted.TrySetResult();
                    await firstActionGate.Task.ConfigureAwait(false);
                }
            },
            out var timer);

        action.Trigger();
        timer.Elapse();
        await firstActionStarted.Task.WaitAsync(ActionTimeout).ConfigureAwait(false);

        action.Trigger();
        timer.Elapse();
        var rerunScheduled = timer.WaitForNextScheduleAsync();
        firstActionGate.TrySetResult();
        await rerunScheduled.WaitAsync(ActionTimeout).ConfigureAwait(false);

        await Assert.That(timer.IsScheduled).IsTrue();
        timer.Elapse();

        await Assert.That(actionCount).IsEqualTo(2);
    }
}
