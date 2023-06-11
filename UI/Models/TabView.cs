using CommunityToolkit.Mvvm.ComponentModel;
using MuseDashModToolsUI.ViewModels;

namespace MuseDashModToolsUI.Models;

public partial class TabView : ObservableObject
{
    [ObservableProperty] private string _displayName;
    public ViewModelBase ViewModel { get; set; }

    public TabView(ViewModelBase viewModel, string displayName)
    {
        ViewModel = viewModel;
        DisplayName = displayName;
    }
}