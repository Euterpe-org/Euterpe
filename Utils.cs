using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MuseDashModToolsUI.Views;

namespace MuseDashModToolsUI;

internal static class Utils
{
    /// <summary>
    /// Used for implementing expanders, cause the built-in one is glitchy as all hell.
    /// </summary>
    internal static void Button_Expander(object? sender, RoutedEventArgs args)
    {
        ((Panel)((Control)sender!).Parent!).Children.First(x => (string)((Control)x).Tag! == "ExpanderContent").IsVisible ^= true;
    }

    internal static void OpenUrl(string linkToOpen)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? linkToOpen : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {linkToOpen}" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? linkToOpen : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
        catch (Exception)
        {
        }
    }

    internal static void OpenPath(string pathToOpen)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? pathToOpen : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-R {pathToOpen}" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? pathToOpen : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
        catch (Exception)
        {
        }
    }

    #region Dialog

    /// <summary>
    /// The following functions are shorthands for opening a new DialogWindow,and then returning the created window
    /// </summary>
    internal static DialogWindow DialogPopup(string message, Action? exitAction, bool isModal = true)
    {
        DialogWindow dialog = new()
        {
            TextDisplay = message,
            WindowExitFunction = exitAction
        };
        if (isModal)
        {
            if (!MainWindow.Instance!.IsVisible)
            {
                MainWindow.Instance.Show();
            }

            dialog.ShowDialog(MainWindow.Instance);
        }
        else
        {
            dialog.Show();
        }

        return dialog;
    }

    internal static DialogWindow DialogPopup(string message, bool isModal = true) => DialogPopup(message, null, isModal);

    #endregion Dialog

    #region Extension Methods

    internal static IBrush ToBrush(this string hexColorString) => (IBrush)new BrushConverter().ConvertFromString(hexColorString)!;

    internal static bool IsValidUrl(this string source) => Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    internal static bool IsValidPath(this string source) => Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeFile;

    #endregion Extension Methods
}