using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.ViewModels;

namespace MuseDashModToolsUI.Models;

public partial class TabView : ObservableObject
{
    [ObservableProperty] private string _displayName;
    public Control View { get; set; }

    public TabView(ViewModelBase viewModel, string displayName)
    {
        View = viewModel.GetView();
        DisplayName = displayName;
    }
}