using CommunityToolkit.Mvvm.ComponentModel;

namespace MuseDashModToolsUI.Models;

public class TabView<T> : ObservableObject
{
    public T ViewModel { get; set; }
    public string DisplayName { get; set; }

    public TabView(T viewModel, string displayName)
    {
        ViewModel = viewModel;
        DisplayName = displayName;
    }
}