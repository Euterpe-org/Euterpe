using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Euterpe.Core.Utils;

public static class DesktopUtils
{
    public static Application GetCurrentApplication()
    {
        var app = Application.Current;
        return app ?? throw new InvalidOperationException("Application is null.");
    }

    public static IClassicDesktopStyleApplicationLifetime GetCurrentDesktop() =>
        GetCurrentApplication().ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
        ?? throw new InvalidOperationException("Desktop is null.");

    public static Window GetCurrentMainWindow()
    {
        var mainWindow = GetCurrentDesktop().MainWindow;
        return mainWindow ?? throw new InvalidOperationException("MainWindow is null.");
    }

    public static void ActivateMainWindow(bool force = false)
    {
        var mainWindow = GetCurrentMainWindow();
        if (mainWindow.WindowState is WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        // This forces window to show at front when using DeepLink
        if (force)
        {
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
        }

        // window.Activate() must be called on UI thread, otherwise it won't work with tray icon
        Dispatcher.UIThread.Post(mainWindow.Activate);
    }
}
