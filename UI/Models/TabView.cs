using CommunityToolkit.Mvvm.ComponentModel;

namespace MuseDashModToolsUI.Models;

public partial class TabView<T> : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public T Item { get; set; }
    public string DisplayName { get; set; }

    public TabView(T item, string displayName, bool isSelected)
    {
        Item = item;
        DisplayName = displayName;
        IsSelected = isSelected;
    }
}