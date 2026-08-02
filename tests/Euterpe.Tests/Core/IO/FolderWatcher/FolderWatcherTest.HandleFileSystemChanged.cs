namespace Euterpe.Tests.Core;

public sealed partial class FolderWatcherTest
{
    [Test]
    public async Task HandleFileSystemChanged_RelevantFileCreated_RunsReconcile()
    {
        var root = Directory.CreateTempSubdirectory("euterpe-fw-").FullName;
        var reconciled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                () =>
                {
                    reconciled.TrySetResult();
                    return Task.CompletedTask;
                },
                Mock.Logger<FolderWatcher>());

            File.WriteAllText(Path.Combine(root, "MyMod.dll"), "x");

            await reconciled.Task.WaitAsync(ReconcileTimeout).ConfigureAwait(false);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task HandleFileSystemChanged_IrrelevantPath_DoesNotRunReconcile()
    {
        var root = Directory.CreateTempSubdirectory("euterpe-fw-").FullName;
        var reconciled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var irrelevantChangeObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                () =>
                {
                    reconciled.TrySetResult();
                    return Task.CompletedTask;
                },
                Mock.Logger<FolderWatcher>(),
                path =>
                {
                    irrelevantChangeObserved.TrySetResult();
                    return path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                });

            File.WriteAllText(Path.Combine(root, "notes.txt"), "x");

            await irrelevantChangeObserved.Task.WaitAsync(ReconcileTimeout).ConfigureAwait(false);
            await Assert.That(reconciled.Task.IsCompleted).IsFalse();
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }
}
