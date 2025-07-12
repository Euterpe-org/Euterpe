using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace MuseDashModTools.Core.Utils;

public static class DesktopUtils
{
    public static Application GetCurrentApplication()
    {
        var app = Application.Current;
        return app ?? throw new InvalidOperationException("Application is null.");
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