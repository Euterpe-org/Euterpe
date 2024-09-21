using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace MuseDashModTools.Utils;

public static class DesktopUtils
{
    public static Application GetCurrentApplication()
    {
        var app = Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application is null.");
        }

        return app;
    }

    public static IClassicDesktopStyleApplicationLifetime GetCurrentDesktop()
    {
        if (GetCurrentApplication().ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            throw new InvalidOperationException("Desktop is null.");
        }

        return desktop;
    }

    public static Window GetCurrentMainWindow()
    {
        var desktop = GetCurrentDesktop();
        if (desktop.MainWindow is null)
        {
            throw new InvalidOperationException("MainWindow is null.");
        }

        return desktop.MainWindow;
    }

    public static TopLevel GetCurrentTopLevel()
    {
        var mainWindow = GetCurrentMainWindow();
        if (TopLevel.GetTopLevel(mainWindow) is not { } topLevel)
        {
            throw new InvalidOperationException("TopLevel is null.");
        }

        return topLevel;
    }

    public static ILauncher GetLauncher() => GetCurrentMainWindow().Launcher;
}