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

    public string TextDisplay
    {
        get => storeTextDisplay;
        set
        {
            storeTextDisplay = value;
            var temp = (Label)MainDialogContainer.Children[0];
            temp.Content = TextDisplay;
        }
    }

    public string storeTextDisplay { get; set; }

    public Action? ButtonClickFunction { get; set; }

    public void Button_Exit(object sender, RoutedEventArgs args)
    {
        ButtonClickFunction?.Invoke();
        Close();
    }

    public event EventHandler<CancelEventArgs> Closing
    {
        add { ButtonClickFunction?.Invoke(); }
        remove { }
    }
}