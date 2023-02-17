using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MuseDashModToolsUI.Views;

public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();
        this.SizeToContent = SizeToContent.WidthAndHeight;
        Closing += CloseWindow;
    }

    internal string TextDisplay
    {
        get => StoreTextDisplay!;
        set
        {
            StoreTextDisplay = value;
            var temp = (TextBlock)MainDialogContainer.Children[0];
            temp.Text = TextDisplay;
        }
    }

    private string? StoreTextDisplay { get; set; }

    internal Action? WindowExitFunction { get; set; }
    internal Action? ButtonClickFunction { get; set; }

    internal void Button_Exit(object? sender, RoutedEventArgs args)
    {
        ButtonClickFunction?.Invoke();
        Close();
    }

    internal void CloseWindow(object? sender, CancelEventArgs args)
    {
        WindowExitFunction?.Invoke();
    }
}