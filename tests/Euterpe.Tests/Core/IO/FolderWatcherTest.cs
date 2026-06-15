using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("FolderWatcherTests")]
[TestSubject(typeof(FolderWatcher))]
public sealed class FolderWatcherTest
{
    [Test]
    public async Task FolderWatcher_FileCreatedInWatchedRoot_InvokesReconcileWithMappedKey()
    {
        var root = Path.Combine(Path.GetTempPath(), $"euterpe-fw-{Guid.NewGuid():N}");
        var changed = new TaskCompletionSource<IReadOnlySet<string>>(TaskCreationOptions.RunContinuationsAsynchronously);

        FolderWatcher? watcher = null;
        try
        {
            watcher = new FolderWatcher(
                [root],
                includeSubdirectories: false,
                mapToKey: static path => path,
                reconcileChanged: paths =>
                {
                    changed.TrySetResult(paths);
                    return Task.CompletedTask;
                },
                reconcileAll: static () => Task.CompletedTask,
                debounce: TimeSpan.FromMilliseconds(50),
                logger: Mock.Logger<FolderWatcher>(),
                label: "test");
            watcher.Start();

            var filePath = Path.Combine(root, "MyMod.dll");
            await File.WriteAllTextAsync(filePath, "x");

            var result = await changed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.That(result).Contains(filePath);
        }
        finally
        {
            watcher?.Dispose();
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
