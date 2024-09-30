using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed partial class PageNavItem(string displayName, string key) : ObservableObject
{
    public PageNavItem[] Children { get; set; } = [];
    public string DisplayName { get; set; } = displayName;
    public string Key { get; set; } = key;
    public bool IsSeparator { get; set; }
    public StreamGeometry? Icon { get; set; }

    public PageNavItem(string displayName) : this(displayName, string.Empty)
    {
    }

    [RelayCommand]
    private void Navigation()
    {
        if (Key.IsNullOrEmpty() || IsSeparator)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(Key, "NavigatePage");
    }
}