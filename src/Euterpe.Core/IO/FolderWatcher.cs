using Lock = System.Threading.Lock;
using Timeout = System.Threading.Timeout;

namespace Euterpe.Core;

// FileSystemWatcher events arrive on thread-pool threads and a single user action emits many of them,
// so changes are debounced on a background timer (never the UI dispatcher) and drained through one serial pump.
internal sealed class FolderWatcher : IDisposable
{
    public static readonly TimeSpan DefaultDebounce = TimeSpan.FromMilliseconds(400);

    private readonly Lock _gate = new();
    private readonly HashSet<string> _dirty = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<FileSystemWatcher> _watchers = [];

    private readonly IReadOnlyList<string> _roots;
    private readonly bool _includeSubdirectories;
    private readonly Func<string, string?> _mapToKey;
    private readonly Func<IReadOnlySet<string>, Task> _reconcileChanged;
    private readonly Func<Task> _reconcileAll;
    private readonly TimeSpan _debounce;
    private readonly ILogger _logger;
    private readonly string _label;
    private readonly Timer _debounceTimer;

    private bool _fullRescanRequested;
    private bool _running;
    private bool _disposed;

    public FolderWatcher(
        IReadOnlyList<string> roots,
        bool includeSubdirectories,
        Func<string, string?> mapToKey,
        Func<IReadOnlySet<string>, Task> reconcileChanged,
        Func<Task> reconcileAll,
        TimeSpan debounce,
        ILogger logger,
        string label)
    {
        _roots = roots;
        _includeSubdirectories = includeSubdirectories;
        _mapToKey = mapToKey;
        _reconcileChanged = reconcileChanged;
        _reconcileAll = reconcileAll;
        _debounce = debounce;
        _logger = logger;
        _label = label;
        _debounceTimer = new Timer(static state => ((FolderWatcher)state!).KickPump(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start()
    {
        foreach (var root in _roots)
        {
            Directory.CreateDirectory(root);
            CreateWatcher(root);
        }
    }

    private void CreateWatcher(string root)
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
        _watchers.Add(watcher);
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e) => Enqueue(e.FullPath);

    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        Enqueue(e.OldFullPath);
        Enqueue(e.FullPath);
    }

    private void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        _logger.ZLogWarning(e.GetException(), $"FileSystemWatcher error on {_label}; re-arming and scheduling a full rescan");

        lock (_gate)
        {
            _fullRescanRequested = true;
        }
        KickPump();

        if (sender is FileSystemWatcher watcher)
        {
            _ = Task.Run(() => RearmWatcher(watcher));
        }
    }

    private void Enqueue(string fullPath)
    {
        if (_mapToKey(fullPath) is not { } key)
        {
            return;
        }

        lock (_gate)
        {
            _dirty.Add(key);
        }
        _debounceTimer.Change(_debounce, Timeout.InfiniteTimeSpan);
    }

    private void KickPump()
    {
        lock (_gate)
        {
            if (_running || _disposed)
            {
                return;
            }
            _running = true;
        }
        _ = PumpAsync();
    }

    private async Task PumpAsync()
    {
        try
        {
            while (true)
            {
                bool full;
                HashSet<string>? batch = null;
                lock (_gate)
                {
                    full = _fullRescanRequested;
                    _fullRescanRequested = false;
                    if (!full && _dirty.Count == 0)
                    {
                        _running = false;
                        return;
                    }

                    if (!full)
                    {
                        batch = new HashSet<string>(_dirty, StringComparer.OrdinalIgnoreCase);
                    }
                    _dirty.Clear();
                }

                try
                {
                    await (full ? _reconcileAll() : _reconcileChanged(batch!)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.ZLogError(ex, $"Reconcile failed on {_label}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Reconcile pump crashed on {_label}");
            lock (_gate)
            {
                _running = false;
            }
        }
    }

    private void RearmWatcher(FileSystemWatcher watcher)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            var root = watcher.Path;
            _watchers.Remove(watcher);
            try
            {
                watcher.Dispose();
                Directory.CreateDirectory(root);
                CreateWatcher(root);
            }
            catch (Exception ex)
            {
                _logger.ZLogError(ex, $"Failed to re-arm watcher on {root}");
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
