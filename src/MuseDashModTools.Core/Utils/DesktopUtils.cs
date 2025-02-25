using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace MuseDashModTools.Core.Utils;

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
}