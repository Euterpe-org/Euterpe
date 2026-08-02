using Euterpe.Shared.Threading;
using Lock = System.Threading.Lock;

namespace Euterpe.Core;

// FileSystemWatcher events arrive on thread-pool threads in bursts, so changes are debounced on a background timer and reconciles never overlap.
internal sealed class FolderWatcher : IDisposable
{
    private static readonly TimeSpan DefaultDebounce = TimeSpan.FromMilliseconds(400);

    private readonly Lock _gate = new();
    private readonly bool _includeSubdirectories;
    private readonly Func<string, bool> _isRelevantChange;
    private readonly ILogger _logger;
    private readonly DebouncedAsyncAction _reconcileAction;
    private readonly List<FileSystemWatcher> _watchers = [];
    private bool _disposed;

    public FolderWatcher(
        IReadOnlyList<string> roots,
        bool includeSubdirectories,
        Func<Task> reconcileAsync,
        ILogger logger,
        Func<string, bool>? isRelevantChange = null,
        TimeSpan? debounce = null)
    {
        _includeSubdirectories = includeSubdirectories;
        _logger = logger;
        _isRelevantChange = isRelevantChange ?? (static _ => true);
        _reconcileAction = new DebouncedAsyncAction(
            debounce ?? DefaultDebounce,
            reconcileAsync,
            ex => _logger.LogError(ex, "Watcher-triggered reconcile failed"));

        foreach (var root in roots)
        {
            Directory.CreateDirectory(root);
            _watchers.Add(CreateWatcher(root));
        }
    }

    public void Dispose()
    {
        List<FileSystemWatcher> watchers;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            watchers = [.. _watchers];
            _watchers.Clear();
        }

        _reconcileAction.Dispose();
        foreach (var watcher in watchers)
        {
            watcher.Dispose();
        }
    }

    private FileSystemWatcher CreateWatcher(string root)
    {
        var watcher = new FileSystemWatcher(root)
        {
            IncludeSubdirectories = _includeSubdirectories,
            InternalBufferSize = 64 * 1024,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        watcher.Created += OnFileSystemChanged;
        watcher.Changed += OnFileSystemChanged;
        watcher.Deleted += OnFileSystemChanged;
        watcher.Renamed += OnFileSystemRenamed;
        watcher.Error += OnFileSystemError;
        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        if (_isRelevantChange(e.FullPath))
        {
            ScheduleReconcile();
        }
    }

    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        if (_isRelevantChange(e.OldFullPath) || _isRelevantChange(e.FullPath))
        {
            ScheduleReconcile();
        }
    }

    private void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        _logger.LogWarning(e.GetException(), "FileSystemWatcher error; re-arming the watcher and scheduling a reconcile");
        RearmWatcher((FileSystemWatcher)sender);
        ScheduleReconcile();
    }

    private void ScheduleReconcile() => _reconcileAction.Trigger();

    private void RearmWatcher(FileSystemWatcher deadWatcher)
    {
        var root = deadWatcher.Path;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _watchers.Remove(deadWatcher);
            deadWatcher.Dispose();
            try
            {
                Directory.CreateDirectory(root);
                _watchers.Add(CreateWatcher(root));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-arm the watcher on {Root}", root);
            }
        }
    }
}
