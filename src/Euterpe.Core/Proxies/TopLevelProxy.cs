using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace Euterpe.Core.Proxies;

public sealed class TopLevelProxy
{
    private TopLevel Current
    {
        get
        {
            var desktop = GetCurrentDesktop();
            return desktop.Windows.LastOrDefault(static w => w.IsVisible)
                   ?? desktop.MainWindow
                   ?? throw new InvalidOperationException("No top-level window available.");
        }
    }

    public IStorageProvider StorageProvider => Current.StorageProvider;
    public ILauncher Launcher => Current.Launcher;
    public IClipboard? Clipboard => Current.Clipboard;
}
