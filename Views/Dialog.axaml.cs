using Avalonia.Controls;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text;
using System;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia;
using ReactiveUI;
using MuseDashModToolsUI.Views;
using System.Diagnostics;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia.Layout;
using System.ComponentModel;

namespace MuseDashModToolsUI.Views
{
    public partial class DialogWindow: Window
    {
        public string TextDisplay
        {
            get
            {
                return storeTextDisplay;
            }
            set
            {
                storeTextDisplay = value;
                var temp = (Label)MainDialogContainer.Children[0];
                temp.Content = TextDisplay;
            }
        }

        public string storeTextDisplay { get; set; }

        public Action? ButtonClickFunction { get; set; }
        public DialogWindow()
        {
            InitializeComponent();
        }

        public void Button_Exit(object sender, RoutedEventArgs args)
        {
            ButtonClickFunction?.Invoke();
            this.Close();
        }

        public event EventHandler<CancelEventArgs> Closing
        {
            add
            {
                ButtonClickFunction?.Invoke();
            }
            remove
            {

            }
        }
    }
}
