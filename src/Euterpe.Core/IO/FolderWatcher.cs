using Lock = System.Threading.Lock;
using Timeout = System.Threading.Timeout;

namespace Euterpe.Core;

// FileSystemWatcher events arrive on thread-pool threads in bursts, so changes are debounced on a background timer and reconciles never overlap.
internal sealed class FolderWatcher : IDisposable
{
    private static readonly TimeSpan DefaultDebounce = TimeSpan.FromMilliseconds(400);

    private readonly Lock _gate = new();
    private readonly List<FileSystemWatcher> _watchers = [];
    private readonly bool _includeSubdirectories;
    private readonly Func<Task> _reconcileAsync;
    private readonly Func<string, bool> _isRelevantChange;
    private readonly TimeSpan _debounce;
    private readonly Timer _debounceTimer;
    private readonly ILogger _logger;

    private bool _reconciling;
    private bool _rerunRequested;
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
        _reconcileAsync = reconcileAsync;
        _logger = logger;
        _isRelevantChange = isRelevantChange ?? (static (string _) => true);
        _debounce = debounce ?? DefaultDebounce;
        _debounceTimer = new Timer(static state => ((FolderWatcher)state!).OnDebounceElapsed(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        foreach (var root in roots)
        {
            Directory.CreateDirectory(root);
            _watchers.Add(CreateWatcher(root));
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
        _logger.ZLogWarning(e.GetException(), $"FileSystemWatcher error; re-arming the watcher and scheduling a reconcile");
        RearmWatcher((FileSystemWatcher)sender);
        ScheduleReconcile();
    }

    private void ScheduleReconcile()
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _debounceTimer.Change(_debounce, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void OnDebounceElapsed()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            if (_reconciling)
            {
                _rerunRequested = true;
                return;
            }
            _reconciling = true;
        }
        _ = RunReconcileAsync();
    }

    private async Task RunReconcileAsync()
    {
        try
        {
            await _reconcileAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Watcher-triggered reconcile failed");
        }

        bool rerun;
        lock (_gate)
        {
            _reconciling = false;
            rerun = _rerunRequested;
            _rerunRequested = false;
        }
        if (rerun)
        {
            ScheduleReconcile();
        }
    }

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
                _logger.ZLogError(ex, $"Failed to re-arm the watcher on {root}");
            }
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

        _debounceTimer.Dispose();
        foreach (var watcher in watchers)
        {
            watcher.Dispose();
        }
    }
}
