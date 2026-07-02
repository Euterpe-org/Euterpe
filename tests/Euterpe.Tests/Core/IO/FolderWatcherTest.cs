namespace Euterpe.Tests.Core;

[Category("FolderWatcherTests")]
[TestSubject(typeof(FolderWatcher))]
public sealed class FolderWatcherTest
{
    private static readonly TimeSpan TestDebounce = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan ReconcileTimeout = TimeSpan.FromSeconds(10);

    private static string CreateTempRoot() => Directory.CreateTempSubdirectory("euterpe-fw-").FullName;

    private static void WriteFile(string root, string fileName) => File.WriteAllText(Path.Combine(root, fileName), "x");

    [Test]
    public async Task FolderWatcher_RelevantFileCreated_RunsReconcile()
    {
        var root = CreateTempRoot();
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
                Mock.Logger<FolderWatcher>(),
                debounce: TestDebounce);

            WriteFile(root, "MyMod.dll");

            await reconciled.Task.WaitAsync(ReconcileTimeout);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task FolderWatcher_IrrelevantFileCreated_DoesNotRunReconcile()
    {
        var root = CreateTempRoot();
        var reconcileCount = 0;
        var reconciled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                () =>
                {
                    Interlocked.Increment(ref reconcileCount);
                    reconciled.TrySetResult();
                    return Task.CompletedTask;
                },
                Mock.Logger<FolderWatcher>(),
                static path => path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase),
                TestDebounce);

            WriteFile(root, "notes.txt");
            await Task.Delay(TestDebounce * 6);
            WriteFile(root, "MyMod.dll");

            await reconciled.Task.WaitAsync(ReconcileTimeout);
            await Task.Delay(TestDebounce * 6);
            await Assert.That(reconcileCount).IsEqualTo(1);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task FolderWatcher_BurstOfChanges_RunsReconcileOnce()
    {
        var root = CreateTempRoot();
        var reconcileCount = 0;
        var reconciled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                () =>
                {
                    Interlocked.Increment(ref reconcileCount);
                    reconciled.TrySetResult();
                    return Task.CompletedTask;
                },
                Mock.Logger<FolderWatcher>(),
                debounce: TimeSpan.FromMilliseconds(250));

            for (var i = 0; i < 5; i++)
            {
                WriteFile(root, $"Mod{i}.dll");
            }

            await reconciled.Task.WaitAsync(ReconcileTimeout);
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            await Assert.That(reconcileCount).IsEqualTo(1);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task FolderWatcher_ReconcileThrows_NextChangeStillRunsReconcile()
    {
        var root = CreateTempRoot();
        var reconcileCount = 0;
        var recovered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                () =>
                {
                    if (Interlocked.Increment(ref reconcileCount) == 1)
                    {
                        throw new InvalidOperationException("reconcile failed");
                    }

                    recovered.TrySetResult();
                    return Task.CompletedTask;
                },
                Mock.Logger<FolderWatcher>(),
                debounce: TestDebounce);

            WriteFile(root, "First.dll");
            await Task.Delay(TestDebounce * 6);
            WriteFile(root, "Second.dll");

            await recovered.Task.WaitAsync(ReconcileTimeout);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task FolderWatcher_ChangesDuringReconcile_RunsReconcileAgain()
    {
        var root = CreateTempRoot();
        var reconcileCount = 0;
        var firstReconcileStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstReconcileGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReconcileRan = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                false,
                async () =>
                {
                    if (Interlocked.Increment(ref reconcileCount) == 1)
                    {
                        firstReconcileStarted.TrySetResult();
                        await firstReconcileGate.Task;
                    }
                    else
                    {
                        secondReconcileRan.TrySetResult();
                    }
                },
                Mock.Logger<FolderWatcher>(),
                debounce: TestDebounce);

            WriteFile(root, "First.dll");
            await firstReconcileStarted.Task.WaitAsync(ReconcileTimeout);

            WriteFile(root, "Second.dll");
            firstReconcileGate.TrySetResult();

            await secondReconcileRan.Task.WaitAsync(ReconcileTimeout);
        }
        finally
        {
            watcher?.Dispose();
            Directory.Delete(root, true);
        }
    }
}
