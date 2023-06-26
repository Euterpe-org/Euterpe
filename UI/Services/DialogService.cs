using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class DialogService : IDialogService
{
    public void ShowDialog(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View")[..^4];
        var type = Type.GetType(name);

        if (type is null) return;
        var dialog = (Window)Activator.CreateInstance(type)!;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            dialog.ShowDialog(desktop.MainWindow!);
    }

    public void ShowDialog<T>()
    {
        var name = typeof(T).FullName!.Replace("ViewModel", "View")[..^4];
        var type = Type.GetType(name);

        if (type is null) return;
        var dialog = (Window)Activator.CreateInstance(type)!;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            dialog.ShowDialog(desktop.MainWindow!);
    }

    public void CloseDialog()
    {
    }
}