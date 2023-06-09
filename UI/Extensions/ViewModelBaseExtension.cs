using System;
using Avalonia.Controls;
using MuseDashModToolsUI.ViewModels;

namespace MuseDashModToolsUI.Extensions;

public static class ViewModelBaseExtension
{
    public static Control GetView(this ViewModelBase viewModel) =>
        (Control)Activator.CreateInstance(Type.GetType(viewModel.GetType().FullName!.Replace("ViewModel", "View"))!)!;
}