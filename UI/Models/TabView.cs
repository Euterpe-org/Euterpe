using CommunityToolkit.Mvvm.ComponentModel;

namespace MuseDashModToolsUI.Models;

public partial class TabView<T> : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public T ViewModel { get; set; }
    public string DisplayName { get; set; }

    public TabView(T viewModel, string displayName, bool isSelected)
    {
        ViewModel = viewModel;
        DisplayName = displayName;
        IsSelected = isSelected;
    }
}