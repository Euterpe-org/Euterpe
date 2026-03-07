using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

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
        var desktop = GetCurrentDesktop();
        var mainWindow = desktop.MainWindow;
        return mainWindow ?? throw new InvalidOperationException("MainWindow is null.");
    }
}