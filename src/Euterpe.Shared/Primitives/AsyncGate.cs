namespace Euterpe.Shared.Primitives;

public sealed class AsyncGate
{
    private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitAsync() => _tcs.Task;
    public void Open() => _tcs.TrySetResult();
}