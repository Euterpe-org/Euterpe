using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Services;

public class DialogService : IDialogService
{
    private Window? _dialog;

    public void ShowDialog(object viewModel)
    {
        var name = viewModel.GetType().FullName!.Replace("ViewModel", "View")[..^4];
        _dialog = CreateWindow(name);
    }

    public void ShowDialog(object viewModel, EventHandler openedEventHandler)
    {
        ShowDialog(viewModel);
        openedEventHandler.Invoke(viewModel, EventArgs.Empty);
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

    public void CloseDialog(EventHandler closedEventHandler)
    {
        CloseDialog();
        closedEventHandler.Invoke(null, EventArgs.Empty);
    }

    private static Window? CreateWindow(string name)
    {
        var type = Type.GetType(name);
        if (type is null) return null;
        var dialog = (Window)Activator.CreateInstance(type)!;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            dialog.Show();
        return dialog;
    }
}