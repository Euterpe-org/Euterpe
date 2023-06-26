using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class DialogService : IDialogService
{
    private Window? _dialog;

    public void ShowDialog(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View")[..^4];
        _dialog = CreateWindow(name);
    }

    public void ShowDialog(object data, EventHandler openedEventHandler)
    {
        ShowDialog(data);
        openedEventHandler.Invoke(data, EventArgs.Empty);
    }

    public void ShowDialog<T>()
    {
        var name = typeof(T).FullName!.Replace("ViewModel", "View")[..^4];
        _dialog = CreateWindow(name);
    }

    public void ShowDialog<T>(EventHandler openedEventHandler)
    {
        ShowDialog<T>();
        openedEventHandler.Invoke(null, EventArgs.Empty);
    }

    public void CloseDialog()
    {
        _dialog?.Close();
        _dialog = null;
    }

    private static Window? CreateWindow(string name)
    {
        var type = Type.GetType(name);
        if (type is null) return null;
        var dialog = (Window)Activator.CreateInstance(type)!;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            dialog.ShowDialog(desktop.MainWindow!);
        return dialog;
    }
}