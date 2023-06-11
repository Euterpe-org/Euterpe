using CommunityToolkit.Mvvm.ComponentModel;

namespace MuseDashModToolsUI.Models;

public class TabView<T> : ObservableObject
{
    [ObservableProperty] private string _displayName;
    public T ViewModel { get; set; }

    public TabView(T viewModel, string displayName)
    {
        ViewModel = viewModel;
        DisplayName = displayName;
    }
}