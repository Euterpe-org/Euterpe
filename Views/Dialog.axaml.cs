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
    }

    internal string TextDisplay
    {
        get => StoreTextDisplay!;
        set
        {
            StoreTextDisplay = value;
            var temp = (Label)MainDialogContainer.Children[0];
            temp.Content = TextDisplay;
        }
    }

    private string? StoreTextDisplay { get; set; }

    internal Action? ButtonClickFunction { get; set; }

    internal void Button_Exit(object sender, RoutedEventArgs args)
    {
        ButtonClickFunction?.Invoke();
        Close();
    }

    internal event EventHandler<CancelEventArgs> Closing
    {
        add { ButtonClickFunction?.Invoke(); }
        remove { }
    }
}